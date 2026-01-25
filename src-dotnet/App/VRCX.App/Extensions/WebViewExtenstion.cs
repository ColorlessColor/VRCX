using VRCX.App.Ipc;
using VRCX.App.WebView;

namespace VRCX.App.Extensions;

public static class WebViewExtenstion
{
    public static void RegisterAppJavascriptObjects(
        this PlatformWebViewControl platformWebViewControl,
        WebViewJsonIpcService webViewJsonIpcService)
    {
        platformWebViewControl.RegisterJavascriptObject("jsonIpcApi", webViewJsonIpcService.IpcHostObject);
    }
}