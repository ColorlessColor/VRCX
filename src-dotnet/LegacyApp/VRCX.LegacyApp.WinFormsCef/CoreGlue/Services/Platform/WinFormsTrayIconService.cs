using VRCX.Core.Services.Platform;
using VRCX.LegacyApp.WinFormsCef.Cef;

namespace VRCX.LegacyApp.WinFormsCef.CoreGlue.Services.Platform;

public sealed class WinFormsTrayIconService : ITrayIconService
{
    public ValueTask SetTrayIconNotificationAsync(bool notify)
    {
        MainForm.Instance.BeginInvoke(new MethodInvoker(() => { MainForm.Instance.SetTrayIconNotification(notify); }));
        return ValueTask.CompletedTask;
    }
}