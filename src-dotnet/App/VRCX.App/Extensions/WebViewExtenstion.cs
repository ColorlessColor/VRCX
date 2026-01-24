using VRCX.App.Ipc;
using VRCX.App.Shared.WebView;

namespace VRCX.App.Extensions;

public static class WebViewExtenstion
{
    public static void RegisterAppJavascriptObjects(this PlatformWebViewControl platformWebViewControl)
    {
        platformWebViewControl.RegisterJavascriptObject("jsonIpcApi", WebViewJsonIpcService.Instance.Interface);
    }
}