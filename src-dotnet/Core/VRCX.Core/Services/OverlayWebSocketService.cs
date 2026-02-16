using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Serilog;
using Serilog.Context;
using VRCX.Core.Models.OverlayWebSocket;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Services;

public sealed class OverlayWebSocketService(
    IMainWebViewService mainWebViewService,
    IOverlayLauncherService overlayLauncherService,
    StartupArgsService startupArgsService
)
{
    private readonly ILogger _logger = Log.ForContext<OverlayWebSocketService>();

    private readonly Lock _sendLock = new();
    private readonly ConcurrentDictionary<WebSocket, byte> _connectedWebSockets = new();
    private CancellationTokenSource? _workerCts;

    private OverlayVars? _overlayVars;

    public async Task StartAsync()
    {
        if (_workerCts is { IsCancellationRequested: true })
            return;

        _workerCts = new CancellationTokenSource();
        try
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:34582/");
            listener.Start();
            _logger.Information("Overlay IPC server started");
            _ = HttpListenerWorkerLoop(listener, _workerCts.Token);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to start Overlay IPC server");
            await _workerCts.CancelAsync();
            _workerCts = null;
        }
    }

    public async Task StopAsync()
    {
        if (_workerCts == null || _workerCts.IsCancellationRequested)
            return;

        foreach (var webSocket in _connectedWebSockets.Keys)
        {
            if (webSocket == null || webSocket.State != WebSocketState.Open)
                continue;

            try
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down",
                    CancellationToken.None);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error closing WebSocket connection during shutdown");
            }
        }

        await _workerCts.CancelAsync();
        _connectedWebSockets.Clear();
        _workerCts = null;
    }

    private async Task HttpListenerWorkerLoop(HttpListener listener, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var listenerContext = await listener.GetContextAsync();
                if (listenerContext.Request.IsWebSocketRequest)
                {
                    _ = ProcessRequest(listenerContext);
                }
                else
                {
                    _logger.Warning(
                        "Received non-WebSocket request to Overlay IPC server from {ClientIp}:{ClientPort}, rejecting",
                        listenerContext.Request.RemoteEndPoint.Address, listenerContext.Request.RemoteEndPoint.Port);

                    listenerContext.Response.StatusCode = 400;
                    listenerContext.Response.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in Overlay IPC listener loop");
            }
        }
    }

    private async Task ProcessRequest(HttpListenerContext listenerContext)
    {
        using (LogContext.PushProperty("WebsocketConnectionId", Guid.NewGuid()))
        {
            WebSocketContext webSocketContext;
            try
            {
                webSocketContext = await listenerContext.AcceptWebSocketAsync(null);
            }
            catch (Exception e)
            {
                listenerContext.Response.StatusCode = 500;
                listenerContext.Response.Close();
                _logger.Error(e, "Failed to accept WebSocket connection");
                return;
            }

            var webSocket = webSocketContext.WebSocket;
            try
            {
                _connectedWebSockets.TryAdd(webSocket, 0);
                _logger.Information("Overlay IPC connected, total connection count: {TotalConnectionCount}",
                    _connectedWebSockets.Count);
                var receiveBuffer = new byte[1024 * 5];
                while (webSocket.State == WebSocketState.Open)
                {
                    var receiveResult =
                        await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                    switch (receiveResult.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            var text = Encoding.UTF8.GetString(receiveBuffer, 0, receiveResult.Count);
                            var message = JsonSerializer.Deserialize<OverlayMessage>(
                                text,
                                OverlayWebSocketJsonContext.Default.OverlayMessage
                            );

                            await HandleMessage(message);
                            continue;

                        case WebSocketMessageType.Close:
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty,
                                CancellationToken.None);
                            break;

                        case WebSocketMessageType.Binary:
                        default:
                            await webSocket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Invalid message type",
                                CancellationToken.None);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error in WebSocket communication loop");
            }
            finally
            {
                webSocket.Dispose();
                _connectedWebSockets.TryRemove(webSocket, out _);
                _logger.Information("Overlay IPC disconnected, total connection count: {TotalConnectionCount}",
                    _connectedWebSockets.Count);
            }
        }
    }

    private async Task HandleMessage(OverlayMessage message)
    {
        using (LogContext.PushProperty("WebSocketMessageType", message.Type))
        using (LogContext.PushProperty("WebSocketRequestId", Guid.NewGuid()))
        {
            _logger.Debug("Overlay IPC message received: {MessageType}", message.Type);
            switch (message.Type)
            {
                case OverlayMessageType.OverlayConnected:
                    var helloMessage = new OverlayMessage
                    {
                        Type = OverlayMessageType.UpdateVars,
                        OverlayVars = _overlayVars
                    };
                    SendMessage(helloMessage);
                    await mainWebViewService.ExecuteScriptAsync("window?.$pinia?.vr.vrInit();");
                    break;

                case OverlayMessageType.IsHmdAfk:
                    var isHmdAfk = string.Equals(message.Data, "true", StringComparison.OrdinalIgnoreCase);
                    await mainWebViewService.ExecuteScriptAsync("window?.$pinia?.game.updateIsHmdAfk", isHmdAfk);
                    break;

                case OverlayMessageType.JsFunctionCall:
                case OverlayMessageType.UpdateVars:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public void SendMessage(OverlayMessage message)
    {
        lock (_sendLock)
        {
            var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)));
            var connectedWebSockets = _connectedWebSockets.Keys;
            _logger.Verbose("Sending {MessageType} message to {ClientCount} overlay Clients",
                message.Type, connectedWebSockets.Count);

            foreach (var webSocket in connectedWebSockets)
            {
                if (webSocket == null || webSocket.State != WebSocketState.Open)
                    continue;

                webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }

    public void UpdateVars(OverlayVars overlayVars)
    {
        _overlayVars = overlayVars;
        if (!IsConnected() && (overlayVars.Active || startupArgsService.LaunchArguments?.IsDebug == true))
        {
            overlayLauncherService.StartOverlay();
            return;
        }

        var helloMessage = new OverlayMessage
        {
            Type = OverlayMessageType.UpdateVars,
            OverlayVars = overlayVars
        };
        SendMessage(helloMessage);
    }

    public bool IsConnected()
    {
        return _connectedWebSockets.Keys.Any(webSocket => webSocket != null && webSocket.State == WebSocketState.Open);
    }
}