using Serilog;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Platform.Linux.Services;

public sealed class LinuxStartupSettingsService : IOsStartupSettingsService
{
    private readonly ILogger _logger = Log.ForContext<LinuxStartupSettingsService>();

    public ValueTask EnableAutoLaunchAsync()
    {
        throw new NotImplementedException();
    }

    public ValueTask DisableAutoLaunchAsync()
    {
        throw new NotImplementedException();
    }
}