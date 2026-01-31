using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using VRCX.App.WebView;
using Xilium.CefGlue.Avalonia;

namespace VRCX.App.Platform.CefDesktop.WebView;

public sealed class CefWebViewControl : PlatformWebViewControl
{
    private readonly AvaloniaCefBrowser _cef;

    private readonly Dictionary<string, object> _javascriptObjects = new();

    public CefWebViewControl()
    {
        _cef = new AvaloniaCefBrowser();

        _cef.LoadStart += (_, _) =>
        {
            foreach (var javascriptObject in _javascriptObjects)
            {
                _cef.RegisterJavascriptObject(javascriptObject.Value, javascriptObject.Key);
            }

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

    public override void RegisterJavascriptObject(string name, object obj)
    {
        _cef.RegisterJavascriptObject(obj, name);

        _javascriptObjects[name] = obj;
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

    public override EventHandler<EventArgs>? NavigationCompleted { get; set; }

    protected override void OnBoundsChanged(Rectangle rectangle)
    {
    }
}