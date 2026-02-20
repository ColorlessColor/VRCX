using Microsoft.Extensions.DependencyInjection;
using VRCX.Core.OverlayClient.OvrToolkit.Services;

namespace VRCX.Core.OverlayClient.OvrToolkit.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddOvrToolkitClient(this IServiceCollection services)
    {
        services.AddSingleton<OvrToolkitClientService>();
        return services;
    }
}