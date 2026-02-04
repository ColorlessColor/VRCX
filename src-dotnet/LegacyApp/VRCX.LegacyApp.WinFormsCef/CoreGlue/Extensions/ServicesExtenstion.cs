using Microsoft.Extensions.DependencyInjection;
using VRCX.Core.Services.Platform;
using VRCX.LegacyApp.WinFormsCef.Cef;
using VRCX.LegacyApp.WinFormsCef.CoreGlue.Services.Platform;

namespace VRCX.LegacyApp.WinFormsCef.CoreGlue.Extensions;

public static class ServicesExtenstion
{
    public static IServiceCollection AddWinFormsCefAppServices(this IServiceCollection services)
    {
        // WinFormsCef Only
        services.AddSingleton<CefService>();

        // Platforms
        services.AddSingleton<WinFormsAppWindowService>();
        services.AddSingleton<WinFormsMainWebViewService>();
        services.AddSingleton<WinFormsNotifyWebLoadedService>();
        services.AddSingleton<WinFormsOverlayLauncherService>();

        services.AddTransient<WinFormsClipboardService>();
        services.AddTransient<WinFormsFileDialogService>();
        services.AddTransient<WinFormsLauncherService>();
        services.AddTransient<WinFormsNativeMessageBoxService>();
        services.AddTransient<WinFormsPlatformLifetimeService>();
        services.AddTransient<WinFormsTrayIconService>();

        services.AddSingleton<IAppWindowService, WinFormsAppWindowService>(s =>
            s.GetRequiredService<WinFormsAppWindowService>());
        services.AddSingleton<IMainWebViewService, WinFormsMainWebViewService>(s =>
            s.GetRequiredService<WinFormsMainWebViewService>());
        services.AddSingleton<INotifyWebLoadedService, WinFormsNotifyWebLoadedService>(s =>
            s.GetRequiredService<WinFormsNotifyWebLoadedService>());
        services.AddSingleton<IOverlayLauncherService, WinFormsOverlayLauncherService>(s =>
            s.GetRequiredService<WinFormsOverlayLauncherService>());

        services.AddTransient<IClipboardService, WinFormsClipboardService>(s =>
            s.GetRequiredService<WinFormsClipboardService>());
        services.AddTransient<IFileDialogService, WinFormsFileDialogService>(s =>
            s.GetRequiredService<WinFormsFileDialogService>());
        services.AddTransient<IPlatformLauncherService, WinFormsLauncherService>(s =>
            s.GetRequiredService<WinFormsLauncherService>());
        services.AddTransient<INativeMessageBoxService, WinFormsNativeMessageBoxService>(s =>
            s.GetRequiredService<WinFormsNativeMessageBoxService>());
        services.AddTransient<IPlatformLifetimeService, WinFormsPlatformLifetimeService>(s =>
            s.GetRequiredService<WinFormsPlatformLifetimeService>());
        services.AddTransient<ITrayIconService, WinFormsTrayIconService>(s =>
            s.GetRequiredService<WinFormsTrayIconService>());

        return services;
    }
}