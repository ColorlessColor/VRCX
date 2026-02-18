using System.Buffers;
using System.Net.WebSockets;
using VRCX.Core.Shared;

namespace VRCX.Core.OverlayClient.WebSocketClient;

public sealed class SimpleWebSocketClient : IDisposable
{
    public SimpleWebSocketClientState State
    {
        get
        {
            return _webSocket?.State switch
            {
#pragma warning disable CS0618
                WebSocketState.None => SimpleWebSocketClientState.None,
#pragma warning restore CS0618
                WebSocketState.Connecting => SimpleWebSocketClientState.Connecting,
                WebSocketState.Open => SimpleWebSocketClientState.Open,
                WebSocketState.CloseSent => SimpleWebSocketClientState.Closing,
                WebSocketState.CloseReceived => SimpleWebSocketClientState.Closing,
                WebSocketState.Closed => SimpleWebSocketClientState.Closing,
                WebSocketState.Aborted => SimpleWebSocketClientState.Closing,
                null => SimpleWebSocketClientState.NoConnection,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public event EventHandler<Exception>? OnReceiveLoopException;
    public event EventHandler? OnWebSocketClosed;

    public event EventHandler<string>? OnTextMessageReceived;

    // NOTE: _cts and _webSocket should be set/unset together. If one of them is null, the other one should also be null.
    // One ClientWebSocket for ONE websocket connection. A new ClientWebSocket instance should be created for each new connection.
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cts;

    private readonly SemaphoreSlim _semaphoreSlim = new(1);
    private bool _disposed;

    public async Task ConnectAsync(Uri websocketUrl, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_webSocket is not null || _cts is not null)
            throw new InvalidOperationException("WebSocket client is already connected.");

        _webSocket = new ClientWebSocket();
        await _webSocket.ConnectAsync(websocketUrl, cancellationToken);

        _cts = new CancellationTokenSource();
        _ = Task.Factory.StartNew(() => ReceiveLoopAsync(_webSocket, _cts.Token), TaskCreationOptions.LongRunning);
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_webSocket is null || _cts is null)
            throw new InvalidOperationException("WebSocket client is not connected.");

        if (_webSocket.State is WebSocketState.Closed or WebSocketState.Aborted)
            throw new InvalidOperationException("WebSocket connection is already closed.");

        await _cts.CancelAsync();
        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", cancellationToken);

        CleanupAfterClose();
    }

    public async Task SendUtf8MessageAsync(string message, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_webSocket is null || _webSocket.State != WebSocketState.Open)
            throw new InvalidOperationException("WebSocket connection is not open.");

        using (await SimpleSemaphoreSlimLockScope.WaitAsync(_semaphoreSlim, cancellationToken))
        {
            var messageBytes = System.Text.Encoding.UTF8.GetBytes(message).AsMemory();
            await _webSocket.SendAsync(messageBytes, WebSocketMessageType.Text, true, cancellationToken);
        }
    }

    private async Task ReceiveLoopAsync(ClientWebSocket webSocket, CancellationToken cancellationToken = default)
    {
        try
        {
            using var buffer = MemoryPool<byte>.Shared.Rent(4096);
            using var fullMessageStream = new MemoryStream();
            using var fullMessageStreamReader = new StreamReader(fullMessageStream);

            var loopState = ReceiveLoopState.WaitingForMessage;
            while (!cancellationToken.IsCancellationRequested)
            {
                // Cancel ReceiverAsync will result in WebSocket Aborted
                var result = await webSocket.ReceiveAsync(buffer.Memory, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    CleanupAfterClose();
                    OnWebSocketClosed?.Invoke(this, EventArgs.Empty);
                    break;
                }

                if (loopState == ReceiveLoopState.WaitingForMessage)
                {
                    loopState = result.MessageType switch
                    {
                        WebSocketMessageType.Text => ReceiveLoopState.ReceivingTextMessage,
                        WebSocketMessageType.Binary => ReceiveLoopState.ReceivingBinaryMessage,
                        _ => throw new InvalidOperationException("Unexpected message type received.")
                    };
                }

                if (loopState == ReceiveLoopState.ReceivingTextMessage &&
                    result.MessageType != WebSocketMessageType.Text)
                    throw new InvalidOperationException("Expected text message type. got " + result.MessageType);

                if (loopState == ReceiveLoopState.ReceivingBinaryMessage &&
                    result.MessageType != WebSocketMessageType.Binary)
                    throw new InvalidOperationException("Expected binary message type. got " + result.MessageType);

                await fullMessageStream.WriteAsync(buffer.Memory[..result.Count], cancellationToken);

                if (result.EndOfMessage)
                {
                    switch (loopState)
                    {
                        case ReceiveLoopState.ReceivingTextMessage:
                            var message = await fullMessageStreamReader.ReadToEndAsync(cancellationToken);
                            OnTextMessageReceived?.Invoke(this, message);
                            break;
                        case ReceiveLoopState.ReceivingBinaryMessage:
                            // Do nothing for now
                            break;
                    }

                    fullMessageStream.SetLength(0);
                    loopState = ReceiveLoopState.WaitingForMessage;
                }
            }
        }
        catch (Exception e)
        {
            webSocket.Abort();
            CleanupAfterClose();
            OnReceiveLoopException?.Invoke(this, e);
        }
    }

    private void CleanupAfterClose()
    {
        _cts?.Dispose();
        _webSocket?.Dispose();
        _cts = null;
        _webSocket = null;
    }

    public void Dispose()
    {
        _disposed = true;

        _webSocket?.Dispose();
        _cts?.Dispose();
        _semaphoreSlim.Dispose();
    }

    private enum ReceiveLoopState
    {
        WaitingForMessage,
        ReceivingTextMessage,
        ReceivingBinaryMessage
    }
}