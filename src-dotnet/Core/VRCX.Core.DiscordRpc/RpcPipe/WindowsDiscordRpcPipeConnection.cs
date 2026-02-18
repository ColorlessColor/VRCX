using System.IO.Pipes;

namespace VRCX.Core.DiscordRpc.RpcPipe;

public sealed class WindowsDiscordRpcPipeConnection : IDiscordRpcPipeConnection, IAsyncDisposable
{
    private NamedPipeClientStream? _pipe;
    private bool _isDisposed;

    public bool IsConnected => _pipe is { IsConnected: true };

    public Stream PipeStream => _pipe ?? throw new InvalidOperationException("Pipe is not connected.");

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (_pipe is { } oldPipe)
        {
            await oldPipe.DisposeAsync();
            _pipe = null;
        }

        List<Exception> exceptions = [];

        // from discord-ipc-0 to discord-ipc-9
        for (var pipeIndex = 0; pipeIndex <= 9; pipeIndex++)
        {
            NamedPipeClientStream? pipe = null;
            try
            {
                pipe = new NamedPipeClientStream(
                    ".",
                    DiscordRpcConst.PipeNamePrefix + pipeIndex,
                    PipeDirection.InOut,
                    PipeOptions.Asynchronous
                );

                await pipe.ConnectAsync(TimeSpan.FromSeconds(2), cancellationToken);
                _pipe = pipe;
                return;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);

                if (pipe is not null)
                    await pipe.DisposeAsync();
            }
        }

        throw new AggregateException("Failed to connect to any Discord RPC pipe.", exceptions);
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_pipe is null) return;

        await _pipe.DisposeAsync();
        _pipe = null;
    }

    public void Dispose()
    {
        _pipe?.Dispose();

        _isDisposed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_pipe != null) await _pipe.DisposeAsync();

        _isDisposed = true;
    }
}