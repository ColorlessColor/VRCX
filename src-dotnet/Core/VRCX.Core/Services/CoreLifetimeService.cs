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

    public async Task StartAsync(string[] args)
    {
        LogManagerExtenstion.Initialize();
        _logger.Info("{AppVersion} Starting...", AppBuildInfoService.Version);

        await startupArgsService.ArgsCheckAsync(args);
        _logger.Info("Args: {LaunchArgsJson}", JsonSerializer.Serialize(startupArgsService.Args));
        if (!string.IsNullOrEmpty(startupArgsService.LaunchArguments?.LaunchCommand))
            _logger.Info("Launch Command: {LaunchCommand}", startupArgsService.LaunchArguments?.LaunchCommand);

        AppPathService.DoMigrationIfNeeded();

        appStorageService.Load();
        sqliteService.Init();
        webApiService.Init();
        logWatcherService.Start();
        discordService.Start();
        processMonitorService.Start();
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