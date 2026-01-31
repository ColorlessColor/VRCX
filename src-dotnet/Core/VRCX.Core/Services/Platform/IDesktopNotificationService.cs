namespace VRCX.Core.Services.Platform;

public interface IDesktopNotificationService
{
    ValueTask SendDesktopNotificationAsync(string title, string? message, string? iconPath = null);
}