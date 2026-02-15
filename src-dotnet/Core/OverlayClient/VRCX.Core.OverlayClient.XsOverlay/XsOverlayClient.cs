using System.Text.Json;
using System.Web;
using VRCX.Core.OverlayClient.WebSocketClient;
using VRCX.Core.OverlayClient.XsOverlay.Models;

namespace VRCX.Core.OverlayClient.XsOverlay;

public sealed class XsOverlayClient : IDisposable
{
    public string ClientName { get; }
    public SimpleWebSocketClientState State => _webSocketClient.State;

    private readonly SimpleWebSocketClient _webSocketClient = new();

    public XsOverlayClient(string clientName)
    {
        ClientName = clientName;
    }

    public async Task ConnectAsync(Uri websocketUrl)
    {
        if (_webSocketClient.State != SimpleWebSocketClientState.NoConnection)
            throw new InvalidOperationException("WebSocket client is already connected or connecting.");

        var uriBuilder = new UriBuilder(websocketUrl);

        var queryCollection = HttpUtility.ParseQueryString("");
        queryCollection.Add("client", ClientName);
        uriBuilder.Query = queryCollection.ToString();

        var uriToConnect = uriBuilder.Uri;
        await _webSocketClient.ConnectAsync(uriToConnect);
    }

    public async Task DisconnectAsync()
    {
        if (_webSocketClient.State != SimpleWebSocketClientState.Open)
            throw new InvalidOperationException("WebSocket client is not connected.");

        await _webSocketClient.DisconnectAsync();
    }

    public async Task SendMessageAsync(XsOverlayWebSocketPayload payload)
    {
        if (_webSocketClient.State != SimpleWebSocketClientState.Open)
            throw new InvalidOperationException("WebSocket client is not connected.");

        await _webSocketClient.SendUtf8MessageAsync(JsonSerializer.Serialize(new XsOverlayWebSocketMessage(
            ClientName,
            payload.Command,
            payload.JsonData,
            payload.RawData,
            payload.Target
        ), XsOverlayWebSocketJsonContext.Default.XsOverlayWebSocketMessage));
    }

    public void Dispose()
    {
        _webSocketClient.Dispose();
    }
}