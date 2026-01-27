using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using VRCX.App.Ipc;
using VRCX.App.ViewModels;
using VRCX.App.Views;
using VRCX.Core.Services;

namespace VRCX.App.Extensions;

public static class ServiceProviderExtenstion
{
    public static void RunApp(this ServiceProvider provider, Func<AppBuilder> buildAvaloniaApp, string[] args)
    {
        using (provider)
        {
            var ipcService = provider.GetRequiredService<WebViewJsonIpcService>();
            var lifetimeService = provider.GetRequiredService<CoreLifetimeService>();

            var bootstrapViewModelFactory = provider.GetRequiredService<BootstrapWindowViewModelFactory>();
            var bootstrapViewModel = bootstrapViewModelFactory.Create(async () =>
            {
                ipcService.RegisterJsonIpcApiObjects(provider);
                await Task.Run(async () => await lifetimeService.StartAsync(args));
            });

            var lifetime = provider.GetRequiredService<ClassicDesktopStyleApplicationLifetime>();

            buildAvaloniaApp().SetupWithLifetime(lifetime);
            lifetime.ShutdownMode = ShutdownMode.OnLastWindowClose;
            lifetime.MainWindow = new BootstrapWindow
            {
                DataContext = bootstrapViewModel
            };

            lifetime.Start(args);

            lifetimeService.Stop();
        }
    }
}