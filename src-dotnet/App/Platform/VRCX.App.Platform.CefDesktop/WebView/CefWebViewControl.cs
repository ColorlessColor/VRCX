using System.Drawing;
using System.Web;
using VRCX.App.WebView;
using Xilium.CefGlue.Avalonia;

namespace VRCX.App.Platform.CefDesktop.WebView;

public sealed class CefWebViewControl : PlatformWebViewControl
{
    private readonly AvaloniaCefBrowser _cef;

    public CefWebViewControl()
    {
        _cef = new AvaloniaCefBrowser();

        _cef.LoadStart += (_, _) =>
        {
            _cef.RegisterJavascriptObject(
                new CefWebViewMessageHostObject(message =>
                {
                    OnMessageReceived?.Invoke(this, new PlatformWebViewMessageEventArgs(message));
                }), "__cefglue_message__");

            _cef.ExecuteJavaScript("""
                                   window.chrome.webview = new class extends EventTarget {
                                       postMessage(message) {
                                           window.__cefglue_message__.postMessage(message);
                                       }
                                   };
                                   """);

            _cef.ShowDeveloperTools();
        };

        _cef.LoadEnd += (_, _) => NavigationCompleted?.Invoke(this, EventArgs.Empty);

        Content = _cef;
    }

    public override Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public override void Navigate(string url)
    {
        _cef.Address = url;
    }

    public override void ExecuteScript(string script)
    {
        _cef.ExecuteJavaScript(script);
    }

    public override void OpenDevTools()
    {
        _cef.ShowDeveloperTools();
    }

    public override ValueTask<double> GetZoomLevelAsync()
    {
        return ValueTask.FromResult(_cef.ZoomLevel);
    }

    public override Task SetZoomLevelAsync(double zoomLevel)
    {
        _cef.ZoomLevel = zoomLevel;
        return Task.CompletedTask;
    }

    public override Task SetDarkModeAsync(bool isDarkMode)
    {
        // TODO: Implement dark mode switch support
        return Task.CompletedTask;
    }

    public override Task SetUserAgentAsync(string userAgent)
    {
        // TODO: Implement user agent override support
        return Task.CompletedTask;
    }

    public override void Close()
    {
        _cef.Dispose();
    }

    public override EventHandler<PlatformWebViewMessageEventArgs>? OnMessageReceived { get; set; }

    public override void PostMessage(string message)
    {
        _cef.ExecuteJavaScript(
            $"window.chrome.webview.dispatchEvent(new CustomEvent('message', {{ detail: \"{HttpUtility.JavaScriptStringEncode(message)}\" }}));");
    }

    public override EventHandler<EventArgs>? NavigationCompleted { get; set; }

    protected override void OnBoundsChanged(Rectangle rectangle)
    {
    }
}