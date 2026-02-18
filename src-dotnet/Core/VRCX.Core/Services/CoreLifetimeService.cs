using Serilog;
using VRCX.Core.Extensions;
using VRCX.Core.OverlayClient.XsOverlay.Services;
using VRCX.Core.Services.AppUpdate;
using VRCX.Core.Services.Ipc;
using VRCX.Core.Shared;

namespace VRCX.Core.Services;

public sealed class CoreLifetimeService(
    SqliteService sqliteService,
    AppStorageService appStorageService,
    WebApi.WebApiService webApiService,
    LogWatcherService logWatcherService,
    DiscordService discordService,
    ProcessMonitorService processMonitorService,
    AppUpdateService appUpdateService,
    OverlayWebSocketService overlayWebSocketService,
    IpcServerService ipcServerService,
    AppWebProxy appWbProxy,
    XsOverlayClientService xsOverlayClientService
)
{
    private readonly ILogger _logger = Log.ForContext<CoreLifetimeService>();

    private static bool _isEarlyPreInitDone;
    private bool _initialized;

    public static void InitBeforeDiContainer(string[] args)
    {
        if (_isEarlyPreInitDone)
            return;

        var launchArgs = StartupArgsService.ArgsCheck(args);
        LogManagerExtenstion.Initialize(
            launchArgs.IsDebug || AppDebugService.InDebugMode,
            launchArgs.IsOverlay ? "overlay" : "app"
        );

        _isEarlyPreInitDone = true;
    }

    public void PreInit(string[] args)
    {
        if (!_isEarlyPreInitDone)
            throw new InvalidOperationException("EarlyPreInit must be called before PreInit.");

        if (_initialized)
            throw new InvalidOperationException("CoreLifetimeService has already been initialized.");

        appWbProxy.Init();

        _logger.Information("{AppVersion} Starting with Args: {LaunchArgsJson}",
            AppBuildInfoService.Version,
            StartupArgsService.Args);

        if (!string.IsNullOrEmpty(StartupArgsService.LaunchArguments?.LaunchCommand))
            _logger.Information("Launch Command: {LaunchCommand}",
                StartupArgsService.LaunchArguments?.LaunchCommand);

        _initialized = true;
    }

    public async Task StartAsync(string[] args)
    {
        if (!_initialized || !_isEarlyPreInitDone)
            throw new InvalidOperationException("CoreLifetimeService must be pre-initialized before starting.");

        await appUpdateService.CompleteInProgressUpdateIfSuccessAsync();

        AppPathService.DoMigrationIfNeeded();

        appStorageService.Load();
        sqliteService.Init();
        webApiService.Init();
        logWatcherService.Start();
        discordService.Start();
        processMonitorService.Start();
        await overlayWebSocketService.StartAsync();
        await ipcServerService.StartAsync();
        await xsOverlayClientService.StartAsync();
    }

    public async Task StopAsync()
    {
        // Dispose are handled by the DI container.
        // "The framework takes on the responsibility of creating an instance of the dependency and disposing of it when it's no longer needed."
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/overview#the-concept
        appStorageService.Save();
        webApiService.SaveCookies();

        await overlayWebSocketService.StopAsync();
        await ipcServerService.StopAsync();
        await xsOverlayClientService.StopAsync();
        await discordService.StopAsync();
    }
}