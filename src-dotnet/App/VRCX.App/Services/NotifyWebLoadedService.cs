using VRCX.Core.Services.Platform;

namespace VRCX.App.Services;

public sealed class NotifyWebLoadedService : INotifyWebLoadedService
{
    private readonly TaskCompletionSource _webLoadedTcs = new();

    public Task WaitForWebLoadedAsync()
    {
        return _webLoadedTcs.Task;
    }

    public ValueTask NotifyWebLoadedAsync()
    {
        _webLoadedTcs.TrySetResult();

        return ValueTask.CompletedTask;
    }
}