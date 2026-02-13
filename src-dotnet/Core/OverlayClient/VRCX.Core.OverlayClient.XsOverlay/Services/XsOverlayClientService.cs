using System.Text.Json;
using Serilog;
using VRCX.Core.OverlayClient.WebSocketClient;
using VRCX.Core.OverlayClient.XsOverlay.Models;
using VRCX.Core.OverlayClient.XsOverlay.Models.CommandPayload;

namespace VRCX.Core.OverlayClient.XsOverlay.Services;

public sealed class XsOverlayClientService
{
    private static readonly Uri XsOverlayWebSocketUri = new("ws://localhost:42070");
    private static readonly string XsOverlayClientName = "vrcx-overlay-client-" + Guid.NewGuid();

    private readonly ILogger _logger = Log.ForContext<XsOverlayClientService>();

    private readonly XsOverlayClient _client = new(XsOverlayClientName);

    private readonly SemaphoreSlim _globalLock = new(1);
    private DateTimeOffset? _connectedAt;

    private readonly TimeSpan _disconnectIfIdleFor = TimeSpan.FromMinutes(1);

    public async Task SendNotificationAsync(XsOverlayWebSocketNotification notification)
    {
        using (await SimpleSemaphoreSlimLockScope.WaitAsync(_globalLock))
        {
            if (_client.State == SimpleWebSocketClientState.Open &&
                (_connectedAt is null || DateTimeOffset.Now - _connectedAt > _disconnectIfIdleFor))
            {
                _logger.Information(
                    "WebSocket client has been idle for more than {IdleTime}, disconnecting and making a new connection",
                    _disconnectIfIdleFor);
                await _client.DisconnectAsync();

                _connectedAt = null;
            }

            if (_client.State == SimpleWebSocketClientState.NoConnection)
            {
                _logger.Information("WebSocket client is not connected, connecting to {Uri}", XsOverlayWebSocketUri);

                try
                {
                    await _client.ConnectAsync(XsOverlayWebSocketUri);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to connect to XsOverlay WebSocket at {Uri}", XsOverlayWebSocketUri);
                    throw;
                }

                _connectedAt = DateTimeOffset.Now;
            }

            if (_client.State != SimpleWebSocketClientState.Open)
            {
                _logger.Error("WebSocket client is in an unexpected state {State} and cannot request connect",
                    _client.State);
                throw new InvalidOperationException(
                    "WebSocket client is not connected and in a state that cannot request connect.");
            }

            _logger.Verbose("Sending notification to XSOverlay, Title: {NotificationTitle}", notification.Title);
            try
            {
                await _client.SendMessageAsync(new XsOverlayWebSocketPayload(
                    "SendNotification",
                    JsonSerializer.Serialize(notification)
                ));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send notification to XsOverlay WebSocket");
                throw;
            }
        }
    }

    public async Task StopAsync()
    {
        if (_client.State == SimpleWebSocketClientState.Open)
        {
            _logger.Information("Stopping XsOverlayClientService and disconnecting WebSocket client");
            await _client.DisconnectAsync();
        }
    }
}