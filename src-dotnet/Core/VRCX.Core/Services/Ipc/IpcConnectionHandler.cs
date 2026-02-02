using System.Globalization;
using System.IO.Pipes;
using Newtonsoft.Json;
using NLog;
using VRCX.Core.Models.Ipc;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Services.Ipc;

public class IpcConnectionHandler : IAsyncDisposable
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly NamedPipeServerStream _namedPipeStream;
    private readonly IMainWebViewService _mainWebViewService;
    private readonly JsonSerializer _serializer = new();

    private readonly CancellationTokenSource _cts = new();
    private bool _isDisposed;
    private bool _started;

    public IpcConnectionHandler(NamedPipeServerStream namedPipeStream, IMainWebViewService mainWebViewService)
    {
        _serializer.Culture = CultureInfo.InvariantCulture;
        _serializer.Formatting = Formatting.None;

        _namedPipeStream = namedPipeStream;
        _mainWebViewService = mainWebViewService;
    }

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_started)
            throw new InvalidOperationException("IPC client already started");

        _started = true;
        _ = Task.Factory.StartNew(() => ReadCoreAsync(_cts.Token), TaskCreationOptions.LongRunning);
    }

    private async Task ReadCoreAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var streamReader = new StreamReader(_namedPipeStream);
            var packetContent = await streamReader.ReadToEndAsync(cancellationToken);

            var packets = packetContent.Split('\0');
            _logger.Info("Received {PacketCount} IPC packets", packets.Length);
            foreach (var packet in packets)
            {
                _logger.Trace("IPC Packet: {Packet}", packet);
                await _mainWebViewService.ExecuteScriptAsync("window?.$pinia?.vrcx.ipcEvent", packet);
            }
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in IPC read loop");
        }

        await DisposeAsync();
    }

    public async ValueTask SendAsync(IpcOutPacket ipcPacket)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        try
        {
            using var memoryStream = new MemoryStream();
            await using var streamWriter = new StreamWriter(memoryStream);
            await using var writer = new JsonTextWriter(streamWriter);

            _serializer.Serialize(writer, ipcPacket);
            await streamWriter.WriteAsync((char)0x00);
            await streamWriter.FlushAsync();

            await memoryStream.CopyToAsync(_namedPipeStream);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error sending IPC packet");
        }
    }

    public async ValueTask DisposeAsync()
    {
        _isDisposed = true;

        await _cts.CancelAsync();
        _cts.Dispose();

        await _namedPipeStream.DisposeAsync();
    }
}