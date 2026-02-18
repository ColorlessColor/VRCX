using Serilog;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Platform.Linux.Services;

public sealed class LinuxStartupSettingsService : IOsStartupSettingsService
{
    private readonly ILogger _logger = Log.ForContext<LinuxStartupSettingsService>();

    public ValueTask EnableAutoLaunchAsync()
    {
        // not implemented
        return ValueTask.CompletedTask;
    }

    public ValueTask DisableAutoLaunchAsync()
    {
        // not implemented
        return ValueTask.CompletedTask;
    }
}