using Microsoft.Extensions.DependencyInjection;
using VRCX.Core;
using VRCX.Core.Services;
using VRCX.LegacyApp.WinFormsCef.CoreGlue.LegacySingleton;
using VRCX.LegacyApp.WinFormsCef.CoreGlue.Services.Platform;
using WebApiService = VRCX.Core.Services.WebApi.WebApiService;

namespace VRCX.LegacyApp.WinFormsCef.CoreGlue.Extensions;

internal static class LegacySingletonExtenstion
{
    public static void InitializeLegacySingletons(this ServiceProvider provider)
    {
        ServiceProviderInstance.Instance = provider;

        StartupArgs.Instance = provider.GetRequiredService<StartupArgsService>();
        VRCXStorage.Instance = provider.GetRequiredService<AppStorageService>();
        WebApi.Instance = provider.GetRequiredService<WebApiService>();
        WebApi.Proxy = provider.GetRequiredService<AppWebProxy>();
        AppWindowService.Instance = provider.GetRequiredService<WinFormsAppWindowService>();
    }
}