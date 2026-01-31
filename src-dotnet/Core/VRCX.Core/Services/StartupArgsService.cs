using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using NLog;

namespace VRCX.Core.Services;

public sealed class StartupArgsService
{
    private const string SubProcessTypeArgument = "--type";

    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public VrcxLaunchArguments? LaunchArguments { get; private set; }
    public string[]? Args { get; private set; }

    public void ArgsCheck(string[] args)
    {
        Args = args;
        Debug.Assert(AppDebugService.InDebugMode);

        LaunchArguments = ParseArgs(args);

        if (LaunchArguments.IsDebug)
            AppDebugService.InDebugMode = true;

        if (LaunchArguments?.ConfigDirectory != null)
        {
            if (File.Exists(LaunchArguments.ConfigDirectory))
            {
                const string message = """
                                       Move your "VRCX.sqlite3" into a folder then specify the folder in the launch parameter e.g.
                                       --config="C:\VRCX\"
                                       """;
                _logger.Fatal(message);
                throw new ArgumentException(message);
            }

            AppPathService.AppDataDirectory = LaunchArguments.ConfigDirectory;
        }

        // var disableClosing = LaunchArguments.IsUpgrade || // we're upgrading, allow it
        //                             !string.IsNullOrEmpty(CommandLineArgsParser.GetArgumentValue(args, CefSharpArguments.SubProcessTypeArgument)); // we're launching a subprocess, allow it
        //
        // // if we're launching a second instance with same config directory, focus the first instance then exit
        // if (!disableClosing && IsDuplicateProcessRunning(LaunchArguments))
        // {
        //     IPCToMain();
        //     Thread.Sleep(10);
        //     Environment.Exit(0);
        // }
    }

    private VrcxLaunchArguments ParseArgs(string[] args)
    {
        var arguments = new VrcxLaunchArguments();
        foreach (var arg in args)
        {
            if (arg == VrcxLaunchArguments.IsStartupPrefix)
                arguments.IsStartup = true;

            if (arg == VrcxLaunchArguments.IsUpgradePrefix)
                arguments.IsUpgrade = true;

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

    private void IPCToMain()
    {
        new IPCServer().CreateIPCServer();
        var ipcClient = new NamedPipeClientStream(".", IPCServer.GetIpcName(), PipeDirection.InOut);
        ipcClient.Connect();

        if (ipcClient.IsConnected)
        {
            var buffer =
                Encoding.UTF8.GetBytes(
                    $"{{\"type\":\"LaunchCommand\",\"command\":\"{LaunchArguments.LaunchCommand}\"}}" + (char)0x00);
            ipcClient.BeginWrite(buffer, 0, buffer.Length, IPCClient.Close, ipcClient);
        }
    }
}

public class VrcxLaunchArguments
{
    public const string IsStartupPrefix = "--startup";
    public bool IsStartup { get; set; } = false;

    public const string IsUpgradePrefix = "/Upgrade";
    public bool IsUpgrade { get; set; } = false;

    public const string IsDebugPrefix = "--debug";
    public bool IsDebug { get; set; } = false;

    public const string Overlay = "--overlay";
    public bool IsOverlay { get; set; } = false;

    public const string LaunchCommandPrefix = "/uri=vrcx://";
    public const string LinuxLaunchCommandPrefix = "vrcx://";
    public string LaunchCommand { get; set; } = null;

    public const string ConfigDirectoryPrefix = "--config";
    public string ConfigDirectory { get; set; } = null;

    public const string ProxyUrlPrefix = "--proxy-server";
    public string ProxyUrl { get; set; } = null;
}