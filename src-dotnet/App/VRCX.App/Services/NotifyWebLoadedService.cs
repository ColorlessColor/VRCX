using Serilog;
using VRCX.Core.Services.Platform;

namespace VRCX.App.Services;

public sealed class NotifyWebLoadedService : INotifyWebLoadedService
{
    private readonly ILogger _logger = Log.ForContext<NotifyWebLoadedService>();

    private readonly TaskCompletionSource _webLoadedTcs = new();
    private readonly CancellationTokenSource _cts = new();

    public NotifyWebLoadedService()
    {
        _cts.Token.Register(() =>
        {
            if (_webLoadedTcs.Task.IsCompleted)
                return;

            _logger.Warning("WaitForWebLoadedAsync timed out, setting result anyway");
            _webLoadedTcs.TrySetResult();
        });
    }

    public Task WaitForWebLoadedAsync()
    {
        _cts.CancelAfter(TimeSpan.FromSeconds(5));
        return _webLoadedTcs.Task;
    }

    public ValueTask NotifyWebLoadedAsync()
    {
        _webLoadedTcs.TrySetResult();

        return ValueTask.CompletedTask;
    }
}