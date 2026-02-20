using System.Collections.Concurrent;
using System.Text.Json;
using Serilog;
using VRCX.Core.OverlayClient.OvrToolkit.Models;
using VRCX.Core.OverlayClient.WebSocketClient;

namespace VRCX.Core.OverlayClient.OvrToolkit.Services;

public sealed class OvrToolkitClientService : IDisposable
{
    private static readonly Uri OvrToolkitWebSocketUri = new("ws://localhost:11450/api");

    private readonly ILogger _logger = Log.ForContext<OvrToolkitClientService>();

    private readonly OvrToolkitClient _client = new();

    private DateTimeOffset? _lastConnectedAt;
    private readonly ConcurrentQueue<OvrToolkitWebSocketMessage> _payloadQueue = new();
    private readonly CancellationTokenSource _loopCts = new();

    private readonly TimeSpan _disconnectIfIdleFor = TimeSpan.FromMinutes(1);

    public Task SendWristNotificationAsync(OvrToolkitWebSocketWristNotificationMessage notification)
    {
        _logger.Verbose("Enqueue send wrist notification to OVRToolkit, Title: {NotificationBody}", notification.Body);
        _payloadQueue.Enqueue(new OvrToolkitWebSocketMessage
        {
            MessageType = "SendWristNotification",
            Json = JsonSerializer.Serialize(
                notification,
                OvrToolkitWebSocketJsonContext.Default.OvrToolkitWebSocketWristNotificationMessage
            )
        });

        return Task.CompletedTask;
    }

    public Task SendHudNotificationAsync(OvrToolkitWebSocketHudNotificationMessage notification)
    {
        _logger.Verbose("Enqueue send HUD notification to OVRToolkit, Title: {NotificationTitle}", notification.Title);
        _payloadQueue.Enqueue(new OvrToolkitWebSocketMessage
        {
            MessageType = "SendNotification",
            Json = JsonSerializer.Serialize(
                notification,
                OvrToolkitWebSocketJsonContext.Default.OvrToolkitWebSocketHudNotificationMessage
            )
        });

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
            try
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
                        "Sending message to OVRToolkit WebSocket server, Command: {PayloadCommand}",
                        payload.MessageType
                    );

                    await _client.SendMessageAsync(payload);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unexpected error in OVRToolkitClientService core loop");
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
                await _client.ConnectAsync(OvrToolkitWebSocketUri);
                _lastConnectedAt = DateTimeOffset.Now;
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "Failed to connect to OVRToolkit WebSocket server at {WebSocketUri}, OVRToolkit may shutdown or not running",
                    OvrToolkitWebSocketUri);

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
            _logger.Information("Stopping OVRToolkitClientService and disconnecting WebSocket client");
            await _loopCts.CancelAsync();
            await _client.DisconnectAsync();
        }
    }

    public void Dispose()
    {
        _client.Dispose();
        _loopCts.Dispose();
    }
}