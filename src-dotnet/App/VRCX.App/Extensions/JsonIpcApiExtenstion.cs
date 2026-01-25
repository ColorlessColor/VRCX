using System;
using Microsoft.Extensions.DependencyInjection;
using VRCX.App.Ipc;

namespace VRCX.App.Extensions;

public static class JsonIpcApiExtenstion
{
    public static void RegisterJsonIpcApiObjects(
        this WebViewJsonIpcService jsonIpcService,
        IServiceProvider serviceProvider)
    {
        jsonIpcService.RegisterJsonIpcObject("AppApi",
            serviceProvider.GetRequiredService<Core.WebViewInterop.App.AppApi>());
        jsonIpcService.RegisterJsonIpcObject("WebApi",
            serviceProvider.GetRequiredService<Core.WebViewInterop.WebApi>());
        jsonIpcService.RegisterJsonIpcObject("VRCXStorage",
            serviceProvider.GetRequiredService<Core.WebViewInterop.VRCXStorage>());
        jsonIpcService.RegisterJsonIpcObject("SQLite",
            serviceProvider.GetRequiredService<Core.WebViewInterop.SQLite>());
        jsonIpcService.RegisterJsonIpcObject("LogWatcher",
            serviceProvider.GetRequiredService<Core.WebViewInterop.LogWatcher>());
        jsonIpcService.RegisterJsonIpcObject("Discord",
            serviceProvider.GetRequiredService<Core.WebViewInterop.Discord>());
        jsonIpcService.RegisterJsonIpcObject("AssetBundleManager",
            serviceProvider.GetRequiredService<Core.WebViewInterop.AssetBundleManager>());
    }
}