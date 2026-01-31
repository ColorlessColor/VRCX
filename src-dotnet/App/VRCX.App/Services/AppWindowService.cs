using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using VRCX.Core.Services.Platform;

namespace VRCX.App.Services;

public class AppWindowService(
    ClassicDesktopStyleApplicationLifetime lifetime,
    MainWebViewService mainWebViewService
    ) : IAppWindowService
{
    private Window? _mainWindow;

    internal void SetMainWindow(Window window)
    {
        _mainWindow = window;
    }

    public TopLevel? GetTopLevel()
    {
        return _mainWindow ?? lifetime.Windows.FirstOrDefault();
    }

    public ValueTask FocusMainWindowAsync()
    {
        _mainWindow?.Activate();
        return ValueTask.CompletedTask;
    }

    public async ValueTask ChangeAppThemeAsync(AppTheme appTheme)
    {
        Application.Current?.RequestedThemeVariant = appTheme switch
        {
            AppTheme.Dark => ThemeVariant.Dark,
            AppTheme.Light => ThemeVariant.Light,
            _ => ThemeVariant.Default
        };
        
        await mainWebViewService.SetDarkModeAsync(appTheme == AppTheme.Dark);
    }
}