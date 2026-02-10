using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Text.Json;
using Serilog;
using VRCX.Core.Models.OverlayWebSocket;
using Websocket.Client;

namespace VRCX.LegacyApp.WinFormsCef.Overlay.Cef;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public static class OverlayClient
{
    private static readonly ILogger Logger = Log.ForContext(typeof(OverlayClient));

    private static readonly Uri WebsocketUri = new("ws://127.0.0.1:34582");
    private static WebsocketClient? _websocketClient;

    public static bool Connected =>
        _websocketClient != null && _websocketClient.IsRunning;
    public static bool ConnectedAndActive =>
        Connected &&
        OverlayProgram.VRCXVRInstance.IsActive();

    public static async Task Init()
    {
        if (_websocketClient != null)
            return;

        var clientFactory = new Func<ClientWebSocket>(() =>
        {
            var client = new ClientWebSocket();
            client.Options.KeepAliveInterval = TimeSpan.FromSeconds(5);
            client.Options.SetRequestHeader("user-agent", "VRCX Overlay Client");
            return client;
        });
        _websocketClient = new WebsocketClient(WebsocketUri, clientFactory)
        {
            Name = "VRCX Overlay Client",
            ReconnectTimeout = null,
            ErrorReconnectTimeout = TimeSpan.FromMilliseconds(300)
        };

        _websocketClient.ReconnectionHappened.Subscribe(info =>
        {
            Logger.Information("Connection happened, type: {0}", info?.Type.ToString());
            var message = new OverlayMessage
            {
                Type = OverlayMessageType.OverlayConnected,
                Data = "VRCX Overlay Client Connected"
            };
            _websocketClient.Send(JsonSerializer.Serialize(message));
        });
        _websocketClient.DisconnectionHappened.Subscribe(info =>
        {
            Logger.Information("Disconnection happened, type: {0}", info?.Type.ToString());
        });
        _websocketClient.MessageReceived.Subscribe(msg =>
        {
            try
            {
                var message = JsonSerializer.Deserialize<OverlayMessage>(msg.Text!);
                HandleMessage(message);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error handling message");
            }
        });

        _websocketClient.Start();
        Logger.Information("VRCX overlay client initialized");
    }

    private static void HandleMessage(OverlayMessage message)
    {
        Logger.Verbose("Message received: {MessageType}", message.Type);
        switch (message.Type)
        {
            case OverlayMessageType.OverlayConnected:
                break;

            case OverlayMessageType.JsFunctionCall:
                OverlayProgram.VRCXVRInstance.ExecuteVrOverlayFunction(message.FunctionName, message.Data);
                break;

            case OverlayMessageType.UpdateVars:
                var overlayVars = message.OverlayVars;
                if (overlayVars == null)
                {
                    Logger.Error("UpdateVars is null");
                    return;
                }
                OverlayProgram.VRCXVRInstance.SetActive(overlayVars.Active, overlayVars.HmdOverlay, overlayVars.WristOverlay,
                    overlayVars.MenuButton, overlayVars.OverlayHand);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

    }

    public static async Task Exit()
    {
        if (_websocketClient == null)
            return;

        _websocketClient.Stop(WebSocketCloseStatus.NormalClosure, "Exiting");
        _websocketClient.Dispose();
        _websocketClient = null;
    }

    public static void SendMessage(OverlayMessage message)
    {
        if (_websocketClient == null || !_websocketClient.IsRunning)
            return;

        _websocketClient.Send(JsonSerializer.Serialize(message));
    }
}
