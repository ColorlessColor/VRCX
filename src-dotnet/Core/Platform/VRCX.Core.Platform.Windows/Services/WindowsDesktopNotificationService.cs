using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Platform.Windows.Services;

public sealed class WindowsDesktopNotificationService : IDesktopNotificationService
{
    private readonly ToastNotifier _toastNotifier =
        ToastNotificationManager.CreateToastNotifier(WindowsConst.AppUserModelId);

    public ValueTask SendDesktopNotificationAsync(string title, string? message, string? iconPath = null)
    {
        var builder = new ToastContentBuilder();

        if (Uri.TryCreate(iconPath, UriKind.Absolute, out var uri))
            builder.AddAppLogoOverride(uri);

        if (!string.IsNullOrEmpty(title))
            builder.AddText(title);

        if (!string.IsNullOrEmpty(message))
            builder.AddText(message);

        _toastNotifier.Show(new ToastNotification(builder.GetXml()));

        return ValueTask.CompletedTask;
    }
}