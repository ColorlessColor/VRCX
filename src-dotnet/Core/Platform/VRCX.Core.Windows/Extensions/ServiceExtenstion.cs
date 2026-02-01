using Microsoft.Extensions.DependencyInjection;
using VRCX.Core.Services.Platform;
using VRCX.Core.Windows.Services;

namespace VRCX.Core.Windows.Extensions;

public static class ServiceExtenstion
{
    public static IServiceCollection AddWindowsPlatformServices(this IServiceCollection services)
    {
        services.AddTransient<IGameFolderProvider, WindowsGameFolderProvider>();
        services.AddTransient<IGameHandlerService, WindowsGameHandlerService>();
        services.AddTransient<IGamePlayPrefsService, WindowsPlayerPrefsService>();
        services.AddTransient<IOsStartupSettingsService, WindowsStartupSettingsService>();
        services.AddTransient<IDesktopNotificationService, WindowsDesktopNotificationService>();
        services.AddTransient<IUpdateInstallationService, WindowsUpdateInstallationService>();

        return services;
    }
}