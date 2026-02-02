using System.Text.Json;
using NLog;
using VRCX.Core.Extensions;
using VRCX.Core.Services.AppUpdate;

namespace VRCX.Core.Services;

public sealed class CoreLifetimeService(
    SqliteService sqliteService,
    AppStorageService appStorageService,
    WebApiService webApiService,
    LogWatcherService logWatcherService,
    DiscordService discordService,
    ProcessMonitorService processMonitorService,
    StartupArgsService startupArgsService,
    AppUpdateService appUpdateService,
    OverlayWebSocketService overlayWebSocketService
)
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private bool _initialized;

    public void PreInit(string[] args)
    {
        if (_initialized)
            throw new InvalidOperationException("CoreLifetimeService has already been initialized.");

        LogManagerExtenstion.Initialize();
        _logger.Info("{AppVersion} Starting...", AppBuildInfoService.Version);

        startupArgsService.ArgsCheck(args);
        _logger.Info("Args: {LaunchArgsJson}", JsonSerializer.Serialize(startupArgsService.Args));
        if (!string.IsNullOrEmpty(startupArgsService.LaunchArguments?.LaunchCommand))
            _logger.Info("Launch Command: {LaunchCommand}", startupArgsService.LaunchArguments?.LaunchCommand);

        _initialized = true;
    }

    public async Task StartAsync(string[] args)
    {
        if (!_initialized)
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
    }

    public async Task StopAsync()
    {
        // Dispose are handled by the DI container.
        // "The framework takes on the responsibility of creating an instance of the dependency and disposing of it when it's no longer needed."
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/overview#the-concept
        appStorageService.Save();
        webApiService.SaveCookies();

        await overlayWebSocketService.StopAsync();
    }
}