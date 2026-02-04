using VRCX.Core.Services.Platform;

namespace VRCX.LegacyApp.WinFormsCef.CoreGlue.Services.Platform;

public sealed class WinFormsNotifyWebLoadedService : INotifyWebLoadedService
{
    public ValueTask NotifyWebLoadedAsync()
    {
        return ValueTask.CompletedTask;
    }
}