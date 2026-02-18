using Microsoft.Extensions.DependencyInjection;
using VRCX.App.WebViewInterop;
using VRCX.Core.WebViewInterop;
using VRCX.Core.WebViewInterop.App;

namespace VRCX.App.Extensions;

public static class JsonIpcApiExtenstion
{
    public static void RegisterJsonIpcApiObjects(
        this WebViewJsonIpcService jsonIpcService,
        IServiceProvider serviceProvider)
    {
        jsonIpcService.RegisterJsonIpcObject("AppApi",
            serviceProvider.GetRequiredService<AppApi>(), typeof(AppApi));
        jsonIpcService.RegisterJsonIpcObject("WebApi",
            serviceProvider.GetRequiredService<WebApi>(), typeof(WebApi));
        jsonIpcService.RegisterJsonIpcObject("VRCXStorage",
            serviceProvider.GetRequiredService<VRCXStorage>(), typeof(VRCXStorage));
        jsonIpcService.RegisterJsonIpcObject("SQLite",
            serviceProvider.GetRequiredService<SQLite>(), typeof(SQLite));
        jsonIpcService.RegisterJsonIpcObject("LogWatcher",
            serviceProvider.GetRequiredService<LogWatcher>(), typeof(LogWatcher));
        jsonIpcService.RegisterJsonIpcObject("Discord",
            serviceProvider.GetRequiredService<Discord>(), typeof(Discord));
        jsonIpcService.RegisterJsonIpcObject("AssetBundleManager",
            serviceProvider.GetRequiredService<AssetBundleManager>(), typeof(AssetBundleManager));
    }
}