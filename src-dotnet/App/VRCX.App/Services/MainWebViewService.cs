using VRCX.App.WebView;
using VRCX.Core.Services.Platform;

namespace VRCX.App.Services;

public sealed class MainWebViewService : IMainWebViewService
{
    private PlatformWebViewControl? _webViewControl;

    internal void SetWebViewControl(PlatformWebViewControl webViewControl)
    {
        _webViewControl = webViewControl;
    }

    public Task ExecuteScriptAsync(string methodName)
    {
        _webViewControl?.ExecuteScript(methodName);
        return Task.CompletedTask;
    }

    public void ShowDevTools()
    {
        _webViewControl?.OpenDevTools();
    }

    public async ValueTask<double> GetZoomLevelAsync()
    {
        if (_webViewControl is null)
            return 100;

        return await _webViewControl.GetZoomLevelAsync();
    }

    public Task SetZoomLevelAsync(double zoomLevel)
    {
        if (_webViewControl is null)
            return Task.CompletedTask;

        return _webViewControl.SetZoomLevelAsync(zoomLevel);
    }

    public async ValueTask SetUserAgentAsync(string userAgent)
    {
        if (_webViewControl is null)
            return;

        await _webViewControl.SetUserAgentAsync(userAgent);
    }

    public async ValueTask SetDarkModeAsync(bool isDarkMode)
    {
        if (_webViewControl is null)
            return;

        await _webViewControl.SetDarkModeAsync(isDarkMode);
    }
}