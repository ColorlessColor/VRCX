using Serilog;
using Serilog.Core;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;

namespace VRCX.Core.Extensions;

public static class LogManagerExtenstion
{
    private static Logger CreateLogger(bool verbose, string appType = "bootstrap")
    {
        var logPath = Path.Join(AppPathService.AppDataDirectory, "logs");

        var jsonLogPath = Path.Combine(logPath, appType, $"vrcx-log-{appType}.json");
        var plainTextLogPath = Path.Combine(logPath, appType, $"vrcx-log-{appType}.log");

        var builder = new LoggerConfiguration();
        if (verbose)
        {
            builder.MinimumLevel.Verbose();
        }
        else
        {
            builder.MinimumLevel.Debug();
        }

        const string plainTextOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:1}] {Message:lj}{NewLine}{Exception}";

        return builder
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "VRCX")
            .Enrich.WithProperty("ApplicationVersion", AppBuildInfoService.Version)
            .Enrich.WithProperty("DebugMode", AppDebugService.InDebugMode)
            .WriteTo.Console(
                applyThemeToRedirectedOutput: true,
                theme: AnsiConsoleTheme.Code,
                outputTemplate: plainTextOutputTemplate)
            .WriteTo.File(new CompactJsonFormatter(), jsonLogPath, rollingInterval: RollingInterval.Day)
            .WriteTo.File(
                plainTextLogPath,
                rollingInterval: RollingInterval.Day,
                outputTemplate: plainTextOutputTemplate)
            .WriteTo.Debug()
            .CreateLogger();
    }

    public static void Initialize(bool verbose, string appType = "app")
    {
        Log.Logger = CreateLogger(verbose, appType);

        // LogManager.Setup().LoadConfiguration(builder =>
        // {
        //     var fileTarget = new FileTarget("fileTarget")
        //     {
        //         FileName = fileName,
        //         //Layout = "${longdate} [${level:uppercase=true}] ${logger} - ${message} ${exception:format=tostring}",
        //         // Layout with padding between the level/logger and message so that the message always starts at the same column
        //         Layout =
        //             "${longdate} [${level:uppercase=true:padding=-5}] ${logger:padding=-20} - ${message} ${exception:format=tostring}",
        //         ArchiveSuffixFormat = "{0:000}",
        //         ArchiveEvery = FileArchivePeriod.Day,
        //         MaxArchiveFiles = 4,
        //         MaxArchiveDays = 7,
        //         ArchiveAboveSize = 10000000,
        //         ArchiveOldFileOnStartup = true,
        //         KeepFileOpen = true,
        //         AutoFlush = true,
        //         Encoding = System.Text.Encoding.UTF8
        //     };
        //     builder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteTo(fileTarget);
        //
        //     var consoleTarget = new ConsoleTarget("consoleTarget")
        //     {
        //         Layout =
        //             "${longdate} [${level:uppercase=true:padding=-5}] ${logger:padding=-20} - ${message} ${exception:format=tostring}",
        //         DetectConsoleAvailable = true
        //     };
        //
        //     builder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteTo(consoleTarget);
        // });
    }
}