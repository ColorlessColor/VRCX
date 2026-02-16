using Microsoft.Extensions.DependencyInjection;
using VRCX.Core.ScreenshotManagement.Services;

namespace VRCX.Core.ScreenshotManagement.Extensions;

public static class ServiceExtension
{
    public static IServiceCollection AddScreenshotManagement(this IServiceCollection services)
    {
        services.AddSingleton<ScreenshotMetadataService>();
        services.AddSingleton<ScreenshotMetadataDatabaseService>();

        return services;
    }
}