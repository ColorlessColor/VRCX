using Microsoft.Extensions.DependencyInjection;
using VRCX.App.Platform.Windows.WebView;
using VRCX.App.WebView;

namespace VRCX.App.Platform.Windows.Extensions;

public static class ServiceExtenstion
{
    public static IServiceCollection AddWindowsWebViewServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlatformWebViewControlFactory, WindowsWebViewControlFactory>();

        return services;
    }
}