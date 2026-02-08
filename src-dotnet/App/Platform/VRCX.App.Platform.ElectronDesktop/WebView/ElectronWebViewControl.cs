using System.Drawing;
using VRCX.App.WebView;

namespace VRCX.App.Platform.ElectronDesktop.WebView;

public sealed class ElectronWebViewControl : PlatformWebViewControl
{
    private readonly ElectronWebViewControlCore _core = new();

    public override async Task InitializeAsync()
    {
        await _core.InitializeAsync();
    }

    public override void Navigate(string url)
    {
        _core.Navigate(url);
    }

    public override void RegisterJavascriptObject(string name, object obj)
    {
        _core.RegisterJavascriptObject(name, obj);
    }

    public override void ExecuteScript(string script)
    {
        _core.ExecuteScript(script);
    }

    public override void OpenDevTools()
    {
        _core.OpenDevTools();
    }

    public override ValueTask<double> GetZoomLevelAsync()
    {
        return ValueTask.FromResult(100d);
    }

    public override Task SetZoomLevelAsync(double zoomLevel)
    {
        return Task.CompletedTask;
    }

    public override Task SetDarkModeAsync(bool isDarkMode)
    {
        return Task.CompletedTask;
    }

    public override Task SetUserAgentAsync(string userAgent)
    {
        return Task.CompletedTask;
    }

    public override EventHandler<EventArgs>? NavigationCompleted { get; set; }

    protected override void OnBoundsChanged(Rectangle rectangle)
    {
    }
}