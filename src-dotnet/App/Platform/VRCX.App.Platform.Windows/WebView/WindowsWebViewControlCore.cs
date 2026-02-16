using System.Drawing;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using Microsoft.Web.WebView2.Core;
using VRCX.App.WebView;

namespace VRCX.App.Platform.Windows.WebView;

internal sealed class WindowsWebViewControlCore(CoreWebView2Environment webView2Environment) : NativeControlHost
{
    public EventHandler<PlatformWebViewMessageEventArgs>? OnMessageReceived { get; set; }
    public EventHandler<EventArgs>? NavigationCompleted { get; set; }

    private readonly TaskCompletionSource<IntPtr> _handlerTcs = new();
    private CoreWebView2Controller? _controller;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var childHandler = base.CreateNativeControlCore(parent);

        _handlerTcs.SetResult(childHandler.Handle);
        return new PlatformHandle(childHandler.Handle, "HWND");
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (IsVisible)
            _controller?.IsVisible = true;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _controller?.IsVisible = false;
    }

    internal async Task InitializeAsync()
    {
        var handle = await _handlerTcs.Task;

        var options = webView2Environment.CreateCoreWebView2ControllerOptions();
        var webView2Controller = await webView2Environment.CreateCoreWebView2ControllerAsync(handle, options);

        webView2Controller.CoreWebView2.NavigationCompleted += (sender, args) =>
        {
            NavigationCompleted?.Invoke(this, EventArgs.Empty);
        };

        webView2Controller.CoreWebView2.AddWebResourceRequestedFilter("https://vrcx/*",
            CoreWebView2WebResourceContext.All);
        webView2Controller.CoreWebView2.WebResourceRequested += (sender, args) =>
        {
            var uri = new Uri(args.Request.Uri);
            var assetsFilePath = uri.LocalPath;
            if (uri.Host == "vrcx")
            {
                try
                {
                    var fileStream = File.OpenRead(Path.Join(AppContext.BaseDirectory, "html", assetsFilePath));
                    var responseStream = new BurnAfterReadStream(fileStream);
                    string headers = "";
                    if (assetsFilePath.EndsWith(".html"))
                    {
                        headers = "Content-Type: text/html";
                    }
                    else if (assetsFilePath.EndsWith(".jpg"))
                    {
                        headers = "Content-Type: image/jpeg";
                    }
                    else if (assetsFilePath.EndsWith(".png"))
                    {
                        headers = "Content-Type: image/png";
                    }
                    else if (assetsFilePath.EndsWith(".css"))
                    {
                        headers = "Content-Type: text/css";
                    }
                    else if (assetsFilePath.EndsWith(".js"))
                    {
                        headers = "Content-Type: application/javascript";
                    }

                    args.Response = _controller?.CoreWebView2.Environment.CreateWebResourceResponse(
                        responseStream, 200, "OK", headers);
                }
                catch
                {
                    args.Response = _controller?.CoreWebView2.Environment.CreateWebResourceResponse(
                        null, 404, "Not found", "");
                }
            }
        };

        webView2Controller.CoreWebView2.WebMessageReceived += OnWebViewMessage;

        _controller = webView2Controller;
    }

    private void OnWebViewMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        OnMessageReceived?.Invoke(this, new PlatformWebViewMessageEventArgs(e.TryGetWebMessageAsString()));
    }

    internal void Navigate(string url)
    {
        _controller?.CoreWebView2.Navigate(url);
    }

    internal void ExecuteScript(string script)
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (_controller is null)
                return;

            try
            {
                await _controller.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        });
    }

    internal void OpenDevTools()
    {
        _controller?.CoreWebView2.OpenDevToolsWindow();
    }

    internal double GetZoomLevel()
    {
        return _controller?.ZoomFactor ?? 100;
    }

    internal void SetZoomLevel(double zoomLevel)
    {
        _controller?.ZoomFactor = zoomLevel;
    }

    public void SetDarkMode(bool isDarkMode)
    {
        _controller?.CoreWebView2.Profile.PreferredColorScheme = isDarkMode
            ? CoreWebView2PreferredColorScheme.Dark
            : CoreWebView2PreferredColorScheme.Light;
    }

    public void SetUserAgent(string userAgent)
    {
        _controller?.CoreWebView2.Settings.UserAgent = userAgent;
    }

    internal void PostMessage(string message)
    {
        _controller?.CoreWebView2.PostWebMessageAsString(message);
    }

    internal void OnBoundsChanged(Rectangle rectangle)
    {
        _controller?.Bounds = rectangle;
    }

    internal void Close()
    {
        _controller?.Close();
    }
}