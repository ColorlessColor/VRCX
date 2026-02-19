using VRCX.Core.Platform.Windows.Interop;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Platform.Windows.Services;

public sealed class WindowsCoreLifetimeService : IPlatformCoreLifetimeService
{
    public Task StartAsync()
    {
        Shell32Interop.SetCurrentProcessExplicitAppUserModelID(WindowsConst.AppUserModelId);

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        return Task.CompletedTask;
    }
}