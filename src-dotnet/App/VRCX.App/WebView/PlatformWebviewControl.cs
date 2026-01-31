using System.Drawing;
using Avalonia;
using Avalonia.Controls;
using VRCX.App.Utils;

namespace VRCX.App.WebView;

public abstract class PlatformWebViewControl : ContentControl
{
    public abstract Task InitializeAsync();
    public abstract void Navigate(string url);
    public abstract void RegisterJavascriptObject(string name, object obj);
    public abstract void ExecuteScript(string script);
    public abstract void OpenDevTools();
    public abstract ValueTask<double> GetZoomLevelAsync();
    public abstract Task SetZoomLevelAsync(double zoomLevel);

    public abstract EventHandler<EventArgs>? NavigationCompleted { get; set; }

    protected abstract void OnBoundsChanged(Rectangle rectangle);

    private Window? _window;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (e.Root is Window window)
            _window = window;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _window = null;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        OnBoundsChanged(GetBounds());
    }

    protected Rectangle GetBounds()
    {
        var screen = BoundsUtils.GetScreenFromWindow(_window);
        var widthPx = BoundsUtils.ToPxSize(Bounds.Width, screen);
        var heightPx = BoundsUtils.ToPxSize(Bounds.Height, screen);

        return new Rectangle(0, 0, widthPx, heightPx);
    }
}