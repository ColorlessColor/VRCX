namespace VRCX.Core.DiscordRpc.RpcPipe;

public interface IDiscordRpcPipeConnection : IDisposable
{
    Task ConnectAsync(CancellationToken cancellationToken);
    Task DisconnectAsync(CancellationToken cancellationToken);

    Stream PipeStream { get; }
    bool IsConnected { get; }
}