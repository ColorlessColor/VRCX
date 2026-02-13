using System.Collections.Concurrent;
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

    private DateTimeOffset? _lastConnectedAt;
    private readonly ConcurrentQueue<XsOverlayWebSocketPayload> _payloadQueue = new();
    private readonly CancellationTokenSource _loopCts = new();

    private readonly TimeSpan _disconnectIfIdleFor = TimeSpan.FromMinutes(1);

    public Task SendNotificationAsync(XsOverlayWebSocketNotification notification)
    {
        _logger.Verbose("Enqueue send notification to XSOverlay, Title: {NotificationTitle}", notification.Title);
        _payloadQueue.Enqueue(new XsOverlayWebSocketPayload(
            "SendNotification",
            JsonSerializer.Serialize(notification)
        ));

        return Task.CompletedTask;
    }

    public Task StartAsync()
    {
        _ = Task.Factory.StartNew(() => CoreLoopAsync(_loopCts.Token), TaskCreationOptions.LongRunning);
        return Task.CompletedTask;
    }

    private async Task CoreLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_payloadQueue.IsEmpty)
            {
                await Task.Delay(1000, cancellationToken);
                continue;
            }

            if (!await EnsureWebSocketConnectedAsync(cancellationToken))
            {
                _logger.Warning("Removing all pending payloads in the queue due to connection failure");
                _payloadQueue.Clear();
                continue;
            }

            while (_payloadQueue.TryDequeue(out var payload))
            {
                _logger.Verbose(
                    "Sending message to XSOverlay WebSocket server, Command: {PayloadCommand}",
                    payload.Command
                );

                await _client.SendMessageAsync(payload);
            }
        }
    }

    private async ValueTask<bool> EnsureWebSocketConnectedAsync(CancellationToken cancellationToken)
    {
        if (_client.State == SimpleWebSocketClientState.Open &&
            (_lastConnectedAt is null || DateTimeOffset.Now - _lastConnectedAt > _disconnectIfIdleFor))
        {
            _logger.Information(
                "WebSocket client has been idle for more than {IdleTime}, disconnecting and making a new connection",
                _disconnectIfIdleFor);
            await _client.DisconnectAsync();
        }

        if (_client.State == SimpleWebSocketClientState.NoConnection)
        {
            try
            {
                await _client.ConnectAsync(XsOverlayWebSocketUri);
                _lastConnectedAt = DateTimeOffset.Now;
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "Failed to connect to XSOverlay WebSocket server at {WebSocketUri}, XSOverlay may shutdown or not running",
                    XsOverlayWebSocketUri);

                return false;
            }
        }

        if (_client.State != SimpleWebSocketClientState.Open)
        {
            _logger.Warning("WebSocket client in {WebSocketClientState} state", _client.State);
            return false;
        }

        return true;
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