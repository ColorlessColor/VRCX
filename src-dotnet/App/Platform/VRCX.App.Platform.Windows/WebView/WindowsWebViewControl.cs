using System.Drawing;
using DirectN.Extensions.Com;
using VRCX.App.WebView;
using WebView2;

namespace VRCX.App.Platform.Windows.WebView;

public sealed class WindowsWebViewControl : PlatformWebViewControl
{
    private readonly WindowsWebViewControlCore _webViewControlCore;

    public WindowsWebViewControl(ComObject<ICoreWebView2Environment15> webView2Environment)
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

    public override Task SetUserAgentAsync(string userAgent)
    {
        _webViewControlCore.SetUserAgent(userAgent);
        return Task.CompletedTask;
    }

    public override void Close()
    {
        _webViewControlCore.Close();
    }

    public override EventHandler<PlatformWebViewMessageEventArgs>? OnMessageReceived
    {
        get => _webViewControlCore.OnMessageReceived;
        set => _webViewControlCore.OnMessageReceived = value;
    }

    public override void PostMessage(string message)
    {
        _webViewControlCore.PostMessage(message);
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