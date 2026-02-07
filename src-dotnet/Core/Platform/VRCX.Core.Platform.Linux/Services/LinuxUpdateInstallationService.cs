using System.Diagnostics;
using NLog;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Platform.Linux.Services;

public sealed class LinuxUpdateInstallationService(
    IPlatformLifetimeService platformLifetimeService
) : IUpdateInstallationService
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    public ValueTask<bool> IsInstallerReadyAsync()
    {
        throw new NotImplementedException();
    }

    public ValueTask PrepareUpdateInstallationAsync(string pathToInstaller)
    {
        throw new NotImplementedException();
    }

    public ValueTask InstallUpdateAsync()
    {
        throw new NotImplementedException();
    }

    public ValueTask CleanupAfterInstallationAsync()
    {
        throw new NotImplementedException();
    }

    public ValueTask CancelUpdateInstallationAsync()
    {
        throw new NotImplementedException();
    }
}