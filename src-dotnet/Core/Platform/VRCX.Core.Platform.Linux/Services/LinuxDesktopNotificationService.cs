using VRCX.Core.Services.Platform;

namespace VRCX.Core.Platform.Linux.Services;

public sealed class LinuxDesktopNotificationService : IDesktopNotificationService
{
    public ValueTask SendDesktopNotificationAsync(string title, string? message, string? iconPath = null)
    {
        throw new NotImplementedException();
    }
}