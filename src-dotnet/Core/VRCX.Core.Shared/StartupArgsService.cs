using System.Diagnostics;

namespace VRCX.Core.Shared;

public static class StartupArgsService
{
    public static VrcxLaunchArguments? LaunchArguments { get; private set; }
    public static string[]? Args { get; private set; }

    public static VrcxLaunchArguments ArgsCheck(string[] args)
    {
        Args = args;
        Debug.Assert(AppDebugService.InDebugMode);

        LaunchArguments = ParseArgs(args);

        if (LaunchArguments.IsDebug)
            AppDebugService.InDebugMode = true;

        if (LaunchArguments.ConfigDirectory != null)
        {
            AppPathService.AppDataDirectory = LaunchArguments.ConfigDirectory;
        }

        return LaunchArguments;
    }

    public static VrcxLaunchArguments ParseArgs(string[] args)
    {
        var arguments = new VrcxLaunchArguments();
        foreach (var arg in args)
        {
            if (arg == VrcxLaunchArguments.IsStartupPrefix)
                arguments.IsStartup = true;

            if (arg.StartsWith(VrcxLaunchArguments.IsDebugPrefix))
                arguments.IsDebug = true;

            if (arg == VrcxLaunchArguments.Overlay)
                arguments.IsOverlay = true;

            if (arg.StartsWith(VrcxLaunchArguments.LaunchCommandPrefix) &&
                arg.Length > VrcxLaunchArguments.LaunchCommandPrefix.Length)
                arguments.LaunchCommand = arg.Substring(VrcxLaunchArguments.LaunchCommandPrefix.Length);

            if (arg.StartsWith(VrcxLaunchArguments.LinuxLaunchCommandPrefix) &&
                arg.Length > VrcxLaunchArguments.LinuxLaunchCommandPrefix.Length)
                arguments.LaunchCommand =
                    arg.Substring(VrcxLaunchArguments.LinuxLaunchCommandPrefix.Length);

            if (arg.StartsWith(VrcxLaunchArguments.ConfigDirectoryPrefix) &&
                arg.Length > VrcxLaunchArguments.ConfigDirectoryPrefix.Length)
                arguments.ConfigDirectory =
                    arg.Substring(VrcxLaunchArguments.ConfigDirectoryPrefix.Length + 1);

            if (arg.StartsWith(VrcxLaunchArguments.ProxyUrlPrefix) &&
                arg.Length > VrcxLaunchArguments.ProxyUrlPrefix.Length)
                arguments.ProxyUrl = arg.Substring(VrcxLaunchArguments.ProxyUrlPrefix.Length + 1)
                    .Replace("'", string.Empty).Replace("\"", string.Empty);
        }

        return arguments;
    }
}

public class VrcxLaunchArguments
{
    public const string IsStartupPrefix = "--startup";
    public bool IsStartup { get; set; }

    public const string IsDebugPrefix = "--debug";
    public bool IsDebug { get; set; }

    public const string Overlay = "--overlay";
    public bool IsOverlay { get; set; }

    public const string LaunchCommandPrefix = "/uri=vrcx://";
    public const string LinuxLaunchCommandPrefix = "vrcx://";
    public string? LaunchCommand { get; set; }

    public const string ConfigDirectoryPrefix = "--config";
    public string? ConfigDirectory { get; set; }

    public const string ProxyUrlPrefix = "--proxy-server";
    public string? ProxyUrl { get; set; }
}