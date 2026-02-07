using NLog;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Platform.Linux.Services;

public sealed class LinuxStartupSettingsService : IOsStartupSettingsService
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    public ValueTask EnableAutoLaunchAsync()
    {
        throw new NotImplementedException();
    }

    public ValueTask DisableAutoLaunchAsync()
    {
        throw new NotImplementedException();
    }
}