using System.Drawing;
using Microsoft.Web.WebView2.Core;
using VRCX.App.WebView;

namespace VRCX.App.Platform.Windows.WebView;

public sealed class WindowsWebViewControl : PlatformWebViewControl
{
    private readonly WindowsWebViewControlCore _webViewControlCore;

    public WindowsWebViewControl(CoreWebView2Environment webView2Environment)
    {
        _webViewControlCore = new WindowsWebViewControlCore(webView2Environment);

        Content = _webViewControlCore;
    }

    public override async Task InitializeAsync()
    {
        await _webViewControlCore.InitializeAsync();
        OnBoundsChanged(GetBounds());
    }

    public override void Navigate(string url)
    {
        _webViewControlCore.Navigate(url);
    }

    public override void RegisterJavascriptObject(string name, object obj)
    {
        _webViewControlCore.RegisterJavascriptObject(name, obj);
    }

    public override void ExecuteScript(string script)
    {
        _webViewControlCore.ExecuteScript(script);
    }

    public override void OpenDevTools()
    {
        _webViewControlCore.OpenDevTools();
    }

    public override ValueTask<double> GetZoomLevelAsync()
    {
        return ValueTask.FromResult(_webViewControlCore.GetZoomLevel());
    }

    public override Task SetZoomLevelAsync(double zoomLevel)
    {
        _webViewControlCore.SetZoomLevel(zoomLevel);
        return Task.CompletedTask;
    }

    public override Task SetDarkModeAsync(bool isDarkMode)
    {
        _webViewControlCore.SetDarkMode(isDarkMode);
        return Task.CompletedTask;
    }

    public override EventHandler<EventArgs>? NavigationCompleted
    {
        get => _webViewControlCore.NavigationCompleted;
        set => _webViewControlCore.NavigationCompleted = value;
    }

    protected override void OnBoundsChanged(Rectangle rectangle)
    {
        _webViewControlCore.OnBoundsChanged(rectangle);
    }
}