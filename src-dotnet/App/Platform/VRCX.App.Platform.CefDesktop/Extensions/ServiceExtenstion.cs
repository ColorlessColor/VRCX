using Microsoft.Extensions.DependencyInjection;
using VRCX.App.Platform.CefDesktop.WebView;
using VRCX.App.WebView;

namespace VRCX.App.Platform.CefDesktop.Extensions;

public static class ServiceExtenstion
{
    public static IServiceCollection AddCefWebViewServices(this IServiceCollection services)
    {
        services.AddSingleton<IPlatformWebViewControlFactory, CefWebViewFactory>();

        return services;
    }
}