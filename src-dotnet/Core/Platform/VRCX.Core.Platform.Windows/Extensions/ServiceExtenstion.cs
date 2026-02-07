using Microsoft.Extensions.DependencyInjection;
using VRCX.Core.Platform.Windows.Services;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Platform.Windows.Extensions;

public static class ServiceExtenstion
{
    public static IServiceCollection AddWindowsPlatformServices(this IServiceCollection services)
    {
        services.AddTransient<IGameFolderProvider, WindowsGameFolderProvider>();
        services.AddTransient<IGameHandlerService, WindowsGameHandlerService>();
        services.AddTransient<IGameRunningStatusService, WindowsGameRunningStatusService>();
        services.AddTransient<IGamePlayPrefsService, WindowsPlayerPrefsService>();
        services.AddTransient<IOsStartupSettingsService, WindowsStartupSettingsService>();
        services.AddTransient<IDesktopNotificationService, WindowsDesktopNotificationService>();
        services.AddTransient<IUpdateInstallationService, WindowsUpdateInstallationService>();

        return services;
    }
}