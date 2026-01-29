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
        services.AddTransient<IPlatformRegistryService, WindowsRegistryService>();
        services.AddTransient<IGamePlayPrefsService, WindowsPlayerPrefsService>();

        return services;
    }
}