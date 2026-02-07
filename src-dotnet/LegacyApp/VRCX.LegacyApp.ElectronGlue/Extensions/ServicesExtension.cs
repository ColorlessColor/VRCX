using Microsoft.Extensions.DependencyInjection;
using VRCX.Core.Services.Platform;
using VRCX.LegacyApp.ElectronGlue.Services.Platform;

namespace VRCX.LegacyApp.ElectronGlue.Extensions;

internal static class ServicesExtension
{
    public static IServiceCollection AddElectronGlueServices(this IServiceCollection services)
    {
        services.AddTransient<ElectronAppWindowService>();
        services.AddTransient<ElectronClipboardService>();
        services.AddTransient<ElectronFileDialogSerivice>();
        services.AddTransient<ElectronLauncherService>();
        services.AddTransient<ElectronMainWebViewService>();
        services.AddTransient<ElectronNativeMessageBoxService>();
        services.AddTransient<ElectronOverlayLauncherService>();
        services.AddTransient<ElectronPlatformLifetimeService>();
        services.AddTransient<ElectronTrayIconService>();
        services.AddTransient<MockNotifyWebLoadedService>();

        services.AddTransient<IAppWindowService>(s => s.GetRequiredService<ElectronAppWindowService>());
        services.AddTransient<IClipboardService>(s => s.GetRequiredService<ElectronClipboardService>());
        services.AddTransient<IFileDialogService>(s => s.GetRequiredService<ElectronFileDialogSerivice>());
        services.AddTransient<IPlatformLauncherService>(s => s.GetRequiredService<ElectronLauncherService>());
        services.AddTransient<IMainWebViewService>(s => s.GetRequiredService<ElectronMainWebViewService>());
        services.AddTransient<INativeMessageBoxService>(s => s.GetRequiredService<ElectronNativeMessageBoxService>());
        services.AddTransient<IOverlayLauncherService>(s => s.GetRequiredService<ElectronOverlayLauncherService>());
        services.AddTransient<ITrayIconService>(s => s.GetRequiredService<ElectronTrayIconService>());
        services.AddTransient<INotifyWebLoadedService>(s => s.GetRequiredService<MockNotifyWebLoadedService>());

        return services;
    }
}