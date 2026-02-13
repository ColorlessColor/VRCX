using Microsoft.Extensions.DependencyInjection;
using VRCX.Core.OverlayClient.XsOverlay.Services;

namespace VRCX.Core.OverlayClient.XsOverlay.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddXsOverlayClient(this IServiceCollection services)
    {
        services.AddSingleton<XsOverlayClientService>();
        return services;
    }
}