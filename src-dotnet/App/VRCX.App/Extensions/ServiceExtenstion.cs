using Avalonia.Controls.ApplicationLifetimes;
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

            services.AddSingleton<ClassicDesktopStyleApplicationLifetime>();

            services.AddSingleton<WebViewJsonIpcService>();
            services.AddSingleton<MainWebViewService>();
            services.AddSingleton<AppWindowService>();
            services.AddTransient<ClipboardService>();
            services.AddSingleton<NativeMessageBoxService>();

            services.AddSingleton<IMainWebViewService>(s => s.GetRequiredService<MainWebViewService>());
            services.AddSingleton<IAppWindowService>(s => s.GetRequiredService<AppWindowService>());
            services.AddTransient<IClipboardService>(s => s.GetRequiredService<ClipboardService>());
            services.AddTransient<INativeMessageBoxService>(s => s.GetRequiredService<NativeMessageBoxService>());
            services.AddSingleton<IFileDialogService, FileDialogService>();

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