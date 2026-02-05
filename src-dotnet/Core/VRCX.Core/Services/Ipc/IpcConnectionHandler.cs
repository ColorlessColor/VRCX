using System.Buffers;
using System.Buffers.Binary;
using System.Globalization;
using System.IO.Pipes;
using System.Text;
using Newtonsoft.Json;
using NLog;
using VRCX.Core.Models.Ipc;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Services.Ipc;

public class IpcConnectionHandler : IAsyncDisposable, IDisposable
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public event EventHandler? OnDisposed;

    private readonly NamedPipeServerStream _namedPipeStream;
    private readonly IMainWebViewService _mainWebViewService;
    private readonly JsonSerializer _serializer = new();

    private readonly CancellationTokenSource _cts = new();
    private bool _isDisposed;
    private bool _started;

    public IpcConnectionHandler(NamedPipeServerStream namedPipeStream, IMainWebViewService mainWebViewService)
    {
        _serializer.Culture = CultureInfo.InvariantCulture;
        _serializer.Formatting = Formatting.None;

        _namedPipeStream = namedPipeStream;
        _mainWebViewService = mainWebViewService;
    }

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_started)
            throw new InvalidOperationException("IPC client already started");

        _started = true;
        _ = Task.Factory.StartNew(() => ReadCoreAsync(_cts.Token), TaskCreationOptions.LongRunning);
    }

    private async Task ReadCoreAsync(CancellationToken cancellationToken)
    {
        try
        {
            Memory<byte> payloadSizeBuffer = new byte[sizeof(int)];
            while (!cancellationToken.IsCancellationRequested)
            {
                if (await _namedPipeStream.ReadAsync(payloadSizeBuffer, cancellationToken) == 0)
                    break;

                var payloadSize = BinaryPrimitives.ReadInt32LittleEndian(payloadSizeBuffer.Span);
                using var payloadBuffer = MemoryPool<byte>.Shared.Rent(payloadSize);

                await _namedPipeStream.ReadExactlyAsync(payloadBuffer.Memory[..payloadSize], cancellationToken);
                var payload = Encoding.UTF8.GetString(payloadBuffer.Memory.Span[..payloadSize]);

                _logger.Debug("IPC Received: {Payload}", payload);
                await _mainWebViewService.ExecuteScriptAsync("window?.$pinia?.vrcx.ipcEvent", payload);
            }
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in IPC read loop");
        }

        await DisposeAsync();
    }

    public async ValueTask SendAsync(IpcOutPacketPayload ipcPacketPayload)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        try
        {
            using var memoryStream = new MemoryStream();
            await using var streamWriter = new StreamWriter(memoryStream);
            await using var writer = new JsonTextWriter(streamWriter);

            _serializer.Serialize(writer, ipcPacketPayload);
            await streamWriter.WriteAsync((char)0x00);
            await streamWriter.FlushAsync();

            await memoryStream.CopyToAsync(_namedPipeStream);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error sending IPC packet");
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        _cts.Cancel();
        _cts.Dispose();

        _namedPipeStream.Dispose();

        OnDisposed?.Invoke(this, EventArgs.Empty);
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        await _cts.CancelAsync();
        _cts.Dispose();

        await _namedPipeStream.DisposeAsync();

        OnDisposed?.Invoke(this, EventArgs.Empty);
    }
}