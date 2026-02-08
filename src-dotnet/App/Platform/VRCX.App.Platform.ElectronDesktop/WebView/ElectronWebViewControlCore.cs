using ElectronNET.API;
using ElectronNET.API.Entities;

namespace VRCX.App.Platform.ElectronDesktop.WebView;

public sealed class ElectronWebViewControlCore
{
    private BrowserWindow? _browserWindow;

    internal async Task InitializeAsync()
    {
        _browserWindow = await Electron.WindowManager.CreateWindowAsync(
            new BrowserWindowOptions
            {
                Show = false,
                WebPreferences = new WebPreferences
                {
                    // WebSecurity = false,
                    // AllowRunningInsecureContent = true
                }
            });

        _browserWindow.Show();
    }

    internal void Navigate(string url)
    {
        _browserWindow?.LoadURL(url);
    }

    internal void ExecuteScript(string script)
    {
        _ = _browserWindow?.WebContents.ExecuteJavaScriptAsync<object>(script, true);
    }

    internal void OpenDevTools()
    {
        _browserWindow?.WebContents.OpenDevTools();
    }
}