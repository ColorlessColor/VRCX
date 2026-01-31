using NLog;
using NLog.Targets;

namespace VRCX.Core.Extensions;

public static class LogManagerExtenstion
{
    public static void Initialize()
    {
        var fileName = Path.Join(AppPathService.AppDataDirectory, "logs", "VRCX.log");
        // if (StartupArgs.LaunchArguments.IsOverlay)
        //     fileName = Path.Join(AppPathService.AppDataDirectory, "logs", "VRCX.Overlay.log");

        LogManager.Setup().LoadConfiguration(builder =>
        {
            var fileTarget = new FileTarget("fileTarget")
            {
                FileName = fileName,
                //Layout = "${longdate} [${level:uppercase=true}] ${logger} - ${message} ${exception:format=tostring}",
                // Layout with padding between the level/logger and message so that the message always starts at the same column
                Layout =
                    "${longdate} [${level:uppercase=true:padding=-5}] ${logger:padding=-20} - ${message} ${exception:format=tostring}",
                ArchiveSuffixFormat = "{0:000}",
                ArchiveEvery = FileArchivePeriod.Day,
                MaxArchiveFiles = 4,
                MaxArchiveDays = 7,
                ArchiveAboveSize = 10000000,
                ArchiveOldFileOnStartup = true,
                KeepFileOpen = true,
                AutoFlush = true,
                Encoding = System.Text.Encoding.UTF8
            };
            builder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteTo(fileTarget);

            var consoleTarget = new ConsoleTarget("consoleTarget")
            {
                Layout =
                    "${longdate} [${level:uppercase=true:padding=-5}] ${logger:padding=-20} - ${message} ${exception:format=tostring}",
                DetectConsoleAvailable = true
            };

            builder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteTo(consoleTarget);
        });
    }
}