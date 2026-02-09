using System.Drawing;
using VRCX.App.WebView;

namespace VRCX.App.Platform.ElectronDesktop.WebView;

public sealed class ElectronWebViewControl : PlatformWebViewControl
{
    private readonly ElectronWebViewControlCore _core = new();

    public ElectronWebViewControl()
    {
        Content = _core;
    }

    public override async Task InitializeAsync()
    {
        await _core.InitializeAsync();
    }

    public override void Navigate(string url)
    {
        _core.Navigate(url);
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

    public override EventHandler<PlatformWebViewMessageEventArgs>? OnMessageReceived
    {
        get => _core.OnMessageReceived;
        set => _core.OnMessageReceived = value;
    }

    public override void PostMessage(string message)
    {
        _core.PostMessage(message);
    }

    public override EventHandler<EventArgs>? NavigationCompleted { get; set; }

    protected override void OnBoundsChanged(Rectangle rectangle)
    {
        _core.OnBoundsChanged(rectangle);
    }
}