using Microsoft.Extensions.DependencyInjection;
using Serilog;
using VRCX.Core;
using VRCX.Core.Ipc;
using VRCX.Core.Services;

namespace VRCX.LegacyApp.WinFormsCef.CoreGlue.Extensions;

internal static class ServiceProviderExtenstion
{
    private static readonly ILogger Logger = Log.ForContext(typeof(ServiceProviderExtenstion));

    public static void RunApp(
        this ServiceProvider provider,
        string[] args,
        Action startupAction,
        Action startupActionForOverlay
    )
    {
        using (provider)
        {
            #region Pre Init

            CoreLifetimeService.EarlyPreInit(args);

            var coreLifetimeService = provider.GetRequiredService<CoreLifetimeService>();
            var startupArgsService = provider.GetRequiredService<StartupArgsService>();

            try
            {
                coreLifetimeService.PreInit(args);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "Fatal Error during PreInit",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                throw;
            }

            #endregion

            if (startupArgsService.LaunchArguments?.IsOverlay == true)
            {
                RunOverlay(startupActionForOverlay);
                return;
            }

            #region Single Instance, Url Handler

            var appMutexScope =
                AppMutexScope.TryEnter(AppMutexScope.AppMutexScopeType.App, AppPathService.AppDataDirectory);
            if (appMutexScope is null)
            {
                if (startupArgsService.LaunchArguments?.LaunchCommand is { } launchCommand)
                {
                    Logger.Debug("Sending launch command to existing instance: {LaunchCommand}", launchCommand);
                    UrlHandlerIpcClient.TrySendUrl(launchCommand);
                    return;
                }

                Logger.Information("Another instance is already running. Exiting this instance");
                return;
            }

            #endregion

            #region Main Application Lifetime

            using (appMutexScope)
            {
                provider.InitializeLegacySingletons();
                coreLifetimeService.StartAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();

                startupAction();

                coreLifetimeService.StopAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            }

            #endregion
        }
    }

    private static void RunOverlay(Action startupActionForOverlay)
    {
        var appMutexScope =
            AppMutexScope.TryEnter(AppMutexScope.AppMutexScopeType.Overlay, AppPathService.AppDataDirectory);
        if (appMutexScope is null)
        {
            Logger.Information("Another overlay instance is already running. Exiting this instance");
            return;
        }

        using (appMutexScope)
        {
            startupActionForOverlay();
        }
    }
}