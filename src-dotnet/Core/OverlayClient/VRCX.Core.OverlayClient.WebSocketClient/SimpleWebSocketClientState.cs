namespace VRCX.Core.OverlayClient.WebSocketClient;

public enum SimpleWebSocketClientState
{
    [Obsolete(
        "Do not use. see https://learn.microsoft.com/en-us/dotnet/api/system.net.websockets.websocketstate?view=net-10.0")]
    None,

    /// <summary>
    /// Ready to make new connection.
    /// </summary>
    NoConnection,
    /// <summary>
    /// The connection is negotiating the handshake with the remote endpoint.
    /// </summary>
    Connecting,
    /// <summary>
    /// The initial state after the HTTP handshake has been completed.
    /// </summary>
    Open,
    /// <summary>
    /// WebSocket Close/Abort in progress
    /// </summary>
    Closing
}