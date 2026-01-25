using Microsoft.Extensions.DependencyInjection;
using VRCX.Core.AppApi;
using VRCX.Core.Services;

namespace VRCX.Core.Extensions;

public static class ServiceExtenstion
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCoreServices()
        {
            services.AddSingleton<AssetBundleService>();
            services.AddSingleton<DiscordService>();
            services.AddSingleton<LogWatcherService>();
            services.AddSingleton<SqliteService>();
            services.AddSingleton<AppStorageService>();
            services.AddSingleton<WebApiService>();
            services.AddSingleton<ProcessMonitorService>();
            services.AddSingleton<AutoAppLaunchService>();

            services.AddSingleton<CoreLifetimeService>();

            services.AddWebViewInteropServices();

            return services;
        }

        private IServiceCollection AddWebViewInteropServices()
        {
            services.AddSingleton<WebViewInterop.App.AppApi, AppApiCore>();
            services.AddSingleton<WebViewInterop.AssetBundleManager>();
            services.AddSingleton<WebViewInterop.Discord>();
            services.AddSingleton<WebViewInterop.LogWatcher>();
            services.AddSingleton<WebViewInterop.SQLite>();
            services.AddSingleton<WebViewInterop.VRCXStorage>();
            services.AddSingleton<WebViewInterop.WebApi>();

            return services;
        }
    }
}