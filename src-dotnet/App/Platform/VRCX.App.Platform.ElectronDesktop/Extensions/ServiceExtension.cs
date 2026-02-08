using Microsoft.Extensions.DependencyInjection;
using VRCX.App.Platform.ElectronDesktop.WebView;
using VRCX.App.WebView;

namespace VRCX.App.Platform.ElectronDesktop.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddElectronServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlatformWebViewControlFactory, ElectronWebViewControlFactory>();

        return services;
    }
}