using Avalonia.Threading;
using VRCX.App.WebViewInterop;
using VRCX.App.WebView;

namespace VRCX.App.Extensions;

public static class WebViewExtenstion
{
    public static void RegisterAppJavascriptObjects(
        this PlatformWebViewControl platformWebViewControl,
        WebViewJsonIpcService webViewJsonIpcService)
    {
        platformWebViewControl.OnMessageReceived += (_, arg) =>
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var result = await webViewJsonIpcService.HandleJsonIpcMessage(arg.Message);
                platformWebViewControl.PostMessage(result);
            });
        };
    }
}