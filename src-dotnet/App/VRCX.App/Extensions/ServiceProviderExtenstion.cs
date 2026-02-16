using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using VRCX.App.WebViewInterop;
using VRCX.App.Services;
using VRCX.App.ViewModels;
using VRCX.App.Views;
using VRCX.Core;
using VRCX.Core.Ipc;
using VRCX.Core.Services;
using VRCX.Core.Services.Platform;
using VRCX.Core.Shared;

namespace VRCX.App.Extensions;

public static class ServiceProviderExtenstion
{
    public static void RunApp(this ServiceProvider provider, Func<AppBuilder> buildAvaloniaApp, string[] args)
    {
        using (provider)
        {
            CoreLifetimeService.EarlyPreInit(args);

            var logger = Log.ForContext(typeof(ServiceProviderExtenstion));

            App.ServiceProvider = provider;

            var ipcService = provider.GetRequiredService<WebViewJsonIpcService>();
            var lifetimeService = provider.GetRequiredService<CoreLifetimeService>();
            var startupArgsService = provider.GetRequiredService<StartupArgsService>();

            Exception? errorDuringPreInit = null;
            try
            {
                lifetimeService.PreInit(args);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "An error occurred during PreInit");
                errorDuringPreInit = ex;
            }

            var appMutexScope =
                AppMutexScope.TryEnter(AppMutexScope.AppMutexScopeType.App, AppPathService.AppDataDirectory);
            if (appMutexScope is null)
            {
                if (startupArgsService.LaunchArguments?.LaunchCommand is { } launchCommand)
                {
                    logger.Debug("Sending launch command to existing instance: {LaunchCommand}", launchCommand);
                    UrlHandlerIpcClient.TrySendUrl(launchCommand);
                    Environment.ExitCode = 0;
                    return;
                }

                logger.Information("Another instance is already running. Exiting this instance");
                Environment.ExitCode = -1;
                return;
            }

            using (appMutexScope)
            {
                var messageBoxService = provider.GetRequiredService<NativeMessageBoxService>();
                var bootstrapViewModelFactory = provider.GetRequiredService<BootstrapWindowViewModelFactory>();
                var bootstrapViewModel = bootstrapViewModelFactory.Create(async () =>
                {
                    if (errorDuringPreInit != null)
                    {
                        await messageBoxService.ShowAsync(
                            errorDuringPreInit.Message,
                            "An error occurred during startup.",
                            NativeMessageBoxIcon.Error);
                        Environment.Exit(1);
                    }

                    ipcService.RegisterJsonIpcApiObjects(provider);
                    await lifetimeService.StartAsync(args).ConfigureAwait(false);
                });

                var lifetime = provider.GetRequiredService<ClassicDesktopStyleApplicationLifetime>();

                buildAvaloniaApp().SetupWithLifetime(lifetime);
                lifetime.ShutdownMode = ShutdownMode.OnLastWindowClose;
                lifetime.MainWindow = new BootstrapWindow
                {
                    DataContext = bootstrapViewModel
                };

                lifetime.Start(args);

                // Do not capture AvaloniaSynchronizationContext, otherwise you will get a deadlock
                Task.Run(async () => await lifetimeService.StopAsync()).GetAwaiter().GetResult();
            }
        }
    }
}