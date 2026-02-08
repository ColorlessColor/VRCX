using ElectronNET.API;
using ElectronNET.API.Entities;
using VRCX.App.WebView;

namespace VRCX.App.Platform.ElectronDesktop.WebView;

public sealed class ElectronWebViewControlCore
{
    private BrowserWindow? _browserWindow;
    private const string WebViewMessageChannel = "webview-message";

    public EventHandler<PlatformWebViewMessageEventArgs>? OnMessageReceived { get; set; }

    internal async Task InitializeAsync()
    {
        _browserWindow = await Electron.WindowManager.CreateWindowAsync(
            new BrowserWindowOptions
            {
                Show = false,
                WebPreferences = new WebPreferences
                {
                    Preload = Path.GetFullPath("preload.js"),
                    ContextIsolation = true
                }
            });

        await Electron.IpcMain.On(WebViewMessageChannel, arg =>
        {
            var message = arg.ToString() ?? "";
            OnMessageReceived?.Invoke(this, new PlatformWebViewMessageEventArgs(message));
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

    internal void PostMessage(string message)
    {
        Electron.IpcMain.Send(_browserWindow, WebViewMessageChannel, message);
    }
}