using Microsoft.Extensions.DependencyInjection;
using VRCX.Core.Platform.Linux.Services;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Platform.Linux.Extensions;

public static class ServiceExtenstion
{
    public static IServiceCollection AddLinuxPlatformServices(this IServiceCollection services)
    {
        services.AddSingleton<LinuxSteamPathService>();
        services.AddTransient<IGameFolderProvider, LinuxGameFolderProvider>();
        services.AddTransient<IGameHandlerService, LinuxGameHandlerService>();
        services.AddTransient<IGameRunningStatusService, LinuxGameRunningStatusService>();
        services.AddTransient<IGamePlayPrefsService, LinuxPlayerPrefsService>();
        services.AddTransient<IOsStartupSettingsService, LinuxStartupSettingsService>();
        services.AddTransient<IDesktopNotificationService, LinuxDesktopNotificationService>();
        services.AddTransient<IUpdateInstallationService, LinuxUpdateInstallationService>();
        services.AddTransient<IPlatformCoreLifetimeService, LinuxCoreLifetimeService>();

        return services;
    }
}