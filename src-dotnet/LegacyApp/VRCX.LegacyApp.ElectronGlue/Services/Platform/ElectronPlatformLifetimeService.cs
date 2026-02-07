using VRCX.Core.Services.Platform;

namespace VRCX.LegacyApp.ElectronGlue.Services.Platform;

public sealed class ElectronPlatformLifetimeService : IPlatformLifetimeService
{
    public ValueTask InvokeShutdownAsync()
    {
        throw new NotImplementedException();
    }

    public ValueTask InvokeRestartAsync()
    {
        throw new NotImplementedException();
    }
}