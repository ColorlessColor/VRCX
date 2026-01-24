using VRCX.App.Ipc;

namespace VRCX.App.Extensions;

public static class JsonIpcApiExtenstion
{
    public static void RegisterJsonIpcApiObjects(this WebViewJsonIpcService jsonIpcService)
    {
        jsonIpcService.RegisterJsonIpcObject("AppApi", Program.AppApiInstance);
        jsonIpcService.RegisterJsonIpcObject("WebApi", WebApi.Instance);
        jsonIpcService.RegisterJsonIpcObject("VRCXStorage", VRCXStorage.Instance);
        jsonIpcService.RegisterJsonIpcObject("SQLite", SQLite.Instance);
        jsonIpcService.RegisterJsonIpcObject("LogWatcher", LogWatcher.Instance);
        jsonIpcService.RegisterJsonIpcObject("Discord", Discord.Instance);
        jsonIpcService.RegisterJsonIpcObject("AssetBundleManager", AssetBundleManager.Instance);
    }
}