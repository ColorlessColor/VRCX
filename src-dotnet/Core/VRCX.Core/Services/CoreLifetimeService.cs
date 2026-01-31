using System;
using System.Text.Json;
using System.Threading.Tasks;
using NLog;
using VRCX.Core.Extensions;

namespace VRCX.Core.Services;

public sealed class CoreLifetimeService(
    SqliteService sqliteService,
    AppStorageService appStorageService,
    WebApiService webApiService,
    LogWatcherService logWatcherService,
    DiscordService discordService,
    ProcessMonitorService processMonitorService,
    StartupArgsService startupArgsService
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

    public Task StartAsync(string[] args)
    {
        if (!_initialized)
            throw new InvalidOperationException("CoreLifetimeService must be pre-initialized before starting.");

        AppPathService.DoMigrationIfNeeded();

        appStorageService.Load();
        sqliteService.Init();
        webApiService.Init();
        logWatcherService.Start();
        discordService.Start();
        processMonitorService.Start();

        return Task.CompletedTask;
    }

    public void Stop()
    {
        // Dispose are handled by the DI container.
        // "The framework takes on the responsibility of creating an instance of the dependency and disposing of it when it's no longer needed."
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/overview#the-concept
        appStorageService.Save();
        webApiService.SaveCookies();
    }
}