using VRCX.Core.Services.Platform;
using VRCX.LegacyApp.WinFormsCef.Cef;

namespace VRCX.LegacyApp.WinFormsCef.CoreGlue.Services.Platform;

public sealed class WinFormsAppWindowService : IAppWindowService
{
    private MainForm? _form;

    internal void SetMainForm(MainForm form)
    {
        _form = form;
    }

    internal MainForm? GetMainForm()
    {
        return _form;
    }

    public async ValueTask FocusMainWindowAsync()
    {
        if (_form is null)
            return;

        await _form.InvokeAsync(() => _form.Focus_Window());
    }

    public async ValueTask ChangeAppThemeAsync(AppTheme appTheme)
    {
        if (_form is null)
            return;

        await _form.InvokeAsync(() =>
        {
            var themeIndex = appTheme switch
            {
                AppTheme.Light => 0,
                AppTheme.Dark => 1,
                _ => 0
            };

            WinformThemer.SetGlobalTheme(themeIndex);
        });
    }
}