using Microsoft.Extensions.DependencyInjection;
using VRCX.App.Ipc;
using VRCX.App.Services;
using VRCX.App.ViewModels;
using VRCX.Core.Extensions;
using VRCX.Core.Services.Platform;

namespace VRCX.App.Extensions;

public static class ServiceExtenstion
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAppServices()
        {
            services.AddCoreServices();

            services.AddSingleton<WebViewJsonIpcService>();
            services.AddSingleton<MainWebViewService>();

            services.AddSingleton<IMainWebViewService>(s => s.GetRequiredService<MainWebViewService>());
            services.AddSingleton<IAppWindowService, AppWindowService>();
            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddSingleton<INativeMessageBoxService, NativeMessageBoxService>();

            services.AddViewModels();

            return services;
        }

        private IServiceCollection AddViewModels()
        {
            services.AddSingleton<BootstrapWindowViewModelFactory>();
            services.AddSingleton<MainWindowViewModel>();

            return services;
        }
    }
}