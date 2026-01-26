using System;
using System.IO;
using NLog;

namespace VRCX.Core;

public static class AppBuildInfoService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static string Version { get; } = GetVersion();

    private static string GetVersion()
    {
        try
        {
            var versionFile = File.ReadAllText(Path.Join(AppPathService.BaseDirectory, "Version")).Trim();

            // look for trailing git hash "-22bcd96" to indicate nightly build
            var version = versionFile.Split('-');
            if (version.Length > 0 && version[^1].Length == 7)
                return $"VRCX Nightly {versionFile}";

            return $"VRCX {versionFile}";
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to read version file");
            return "VRCX Nightly Build";
        }
    }
}