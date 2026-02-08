using Microsoft.Toolkit.Uwp.Notifications;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Platform.Windows.Services;

public sealed class WindowsDesktopNotificationService : IDesktopNotificationService
{
    public ValueTask SendDesktopNotificationAsync(string title, string? message, string? iconPath = null)
    {
        var builder = new ToastContentBuilder();

        if (Uri.TryCreate(iconPath, UriKind.Absolute, out var uri))
            builder.AddAppLogoOverride(uri);

        if (!string.IsNullOrEmpty(title))
            builder.AddText(title);

        if (!string.IsNullOrEmpty(message))
            builder.AddText(message);

        builder.Show();

        return ValueTask.CompletedTask;
    }
}