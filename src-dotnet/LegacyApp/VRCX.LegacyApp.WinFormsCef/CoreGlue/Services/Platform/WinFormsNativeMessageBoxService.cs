using VRCX.Core.Services.Platform;
using VRCX.LegacyApp.WinFormsCef.CoreGlue.Utils;

namespace VRCX.LegacyApp.WinFormsCef.CoreGlue.Services.Platform;

public sealed class WinFormsNativeMessageBoxService : INativeMessageBoxService
{
    public async Task ShowAsync(string message, string title, NativeMessageBoxIcon icon)
    {
        var iconType = icon switch
        {
            NativeMessageBoxIcon.Error => MessageBoxIcon.Error,
            _ => MessageBoxIcon.None,
        };

        await StaThreadUtils.RunOnStaThread(() =>
            MessageBox.Show(message, title, MessageBoxButtons.OK, iconType)
        );
    }
}