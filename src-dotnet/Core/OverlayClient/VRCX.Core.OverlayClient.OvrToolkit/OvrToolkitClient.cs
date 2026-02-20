using System.Text.Json;
using VRCX.Core.OverlayClient.OvrToolkit.Models;
using VRCX.Core.OverlayClient.WebSocketClient;

namespace VRCX.Core.OverlayClient.OvrToolkit;

public sealed class OvrToolkitClient : IDisposable
{
    public SimpleWebSocketClientState State => _webSocketClient.State;

    private readonly SimpleWebSocketClient _webSocketClient = new();

    public async Task ConnectAsync(Uri websocketUrl)
    {
        if (_webSocketClient.State != SimpleWebSocketClientState.NoConnection)
            throw new InvalidOperationException("WebSocket client is already connected or connecting.");

        await _webSocketClient.ConnectAsync(websocketUrl);
    }

    public async Task DisconnectAsync()
    {
        if (_webSocketClient.State != SimpleWebSocketClientState.Open)
            throw new InvalidOperationException("WebSocket client is not connected.");

        await _webSocketClient.DisconnectAsync();
    }

    public async Task SendMessageAsync(OvrToolkitWebSocketMessage payload)
    {
        if (_webSocketClient.State != SimpleWebSocketClientState.Open)
            throw new InvalidOperationException("WebSocket client is not connected.");

        await _webSocketClient.SendUtf8MessageAsync(JsonSerializer.Serialize(
            payload,
            OvrToolkitWebSocketJsonContext.Default.OvrToolkitWebSocketMessage
        ));
    }

    public void Dispose()
    {
        _webSocketClient.Dispose();
    }
}