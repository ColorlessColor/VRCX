using System;
using System.Drawing;
using System.Threading.Tasks;
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