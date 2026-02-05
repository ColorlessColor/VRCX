using System.IO.Pipes;
using NLog;
using VRCX.Core.Models.Ipc;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Services.Ipc;

public sealed class IpcServerService(
    IMainWebViewService mainWebViewService
)
{
    public const string IpcPipeName = "vrcx-ipc-01d77b16";

    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly Lock _clientsLock = new();
    private readonly List<IpcConnectionHandler> _clients = [];

    private CancellationTokenSource? _serverLoopCts;

    public async Task StartAsync()
    {
        if (_serverLoopCts is not null)
        {
            await _serverLoopCts.CancelAsync();
            _serverLoopCts.Dispose();
        }

        _serverLoopCts = new CancellationTokenSource();
        _ = Task.Factory.StartNew(() => ServerLoopCoreAsync(_serverLoopCts.Token), TaskCreationOptions.LongRunning);
    }

    public void Stop()
    {
        if (_serverLoopCts is not null)
        {
            _serverLoopCts.Cancel();
            _serverLoopCts.Dispose();
            _serverLoopCts = null;
        }

        IpcConnectionHandler[] clientsCopy;
        lock (_clientsLock)
        {
            clientsCopy = _clients.ToArray();
            _clients.Clear();
        }

        foreach (var connection in clientsCopy)
        {
            connection.Dispose();
        }
    }

    private async Task ServerLoopCoreAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            NamedPipeServerStream? serverPipeStream = null;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                serverPipeStream = new NamedPipeServerStream(
                    IpcPipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                await serverPipeStream.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

                var connection = new IpcConnectionHandler(serverPipeStream, mainWebViewService);
                connection.OnDisposed += (_, _) =>
                {
                    lock (_clientsLock)
                    {
                        _clients.Remove(connection);
                    }
                };

                lock (_clientsLock)
                {
                    _clients.Add(connection);
                }

                connection.Start();
            }
            catch (OperationCanceledException)
            {
                if (serverPipeStream is not null)
                    await serverPipeStream.DisposeAsync();

                return;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "IPC Server Loop Error");

                if (serverPipeStream is not null)
                    await serverPipeStream.DisposeAsync();
            }
        }
    }

    public async ValueTask SendAsync(IpcOutPacketPayload ipcPacketPayload)
    {
        IpcConnectionHandler[] clientsCopy;
        lock (_clientsLock)
        {
            clientsCopy = _clients.ToArray();
        }

        foreach (var client in clientsCopy)
        {
            try
            {
                await client.SendAsync(ipcPacketPayload);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error sending IPC packet to client");
            }
        }
    }
}