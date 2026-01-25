using System;
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
            Program.Init("snapshot", ["--debug"]);

            var ipcService = provider.GetRequiredService<WebViewJsonIpcService>();
            ipcService.RegisterJsonIpcApiObjects(provider);

            var lifetimeService = provider.GetRequiredService<CoreLifetimeService>();
            lifetimeService.Start();

            var mainWindowsViewModel = provider.GetRequiredService<MainWindowViewModel>();

            var lifetime = new ClassicDesktopStyleApplicationLifetime
            {
                ShutdownMode = ShutdownMode.OnMainWindowClose
            };

            buildAvaloniaApp().SetupWithLifetime(lifetime);
            lifetime.MainWindow = new MainWindow
            {
                DataContext = mainWindowsViewModel
            };

            lifetime.Start();

            lifetimeService.Stop();   
        }
    }
}