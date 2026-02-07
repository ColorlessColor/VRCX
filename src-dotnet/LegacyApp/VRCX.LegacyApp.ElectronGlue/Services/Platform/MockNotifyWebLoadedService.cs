using VRCX.Core.Services.Platform;

namespace VRCX.LegacyApp.ElectronGlue.Services.Platform;

public sealed class MockNotifyWebLoadedService : INotifyWebLoadedService
{
    public ValueTask NotifyWebLoadedAsync()
    {
        return ValueTask.CompletedTask;
    }
}