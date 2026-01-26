using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Services;

public sealed class StartupArgsService(INativeMessageBoxService messageBoxService)
{
    private const string SubProcessTypeArgument = "--type";

    public VrcxLaunchArguments? LaunchArguments { get; private set; }
    public string[]? Args { get; private set; }

    public async Task ArgsCheckAsync(string[] args)
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
                var message =
                    "Move your \"VRCX.sqlite3\" into a folder then specify the folder in the launch parameter e.g.\n--config=\"C:\\VRCX\\\"";
                Console.WriteLine(message);
                await messageBoxService.ShowAsync(message, "--config is now a directory", NativeMessageBoxIcon.Error);
                Environment.Exit(0);
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

    private bool IsDuplicateProcessRunning(VrcxLaunchArguments launchArguments)
    {
        var processes = Process.GetProcessesByName("VRCX");
        var isDuplicateProcessRunning = false;
        foreach (var process in processes)
        {
            if (process.Id == Environment.ProcessId)
                continue;

            var commandLine = string.Empty;
            try
            {
                using var searcher =
                    new ManagementObjectSearcher(
                        "SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id);
                using var objects = searcher.Get();
                commandLine =
                    objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString() ??
                    string.Empty;
            }
            catch
            {
                // ignored
            }

            if (commandLine.Contains(SubProcessTypeArgument)) // ignore subprocesses
                continue;

            if (launchArguments.IsOverlay)
            {
                if (commandLine.Contains(VrcxLaunchArguments.Overlay))
                {
                    Console.WriteLine(@"Another overlay instance is already running. Exiting this instance.");
                    Environment.Exit(0);
                }

                continue; // we are an overlay, ignore non-overlay instances
            }

            if (commandLine.Contains(VrcxLaunchArguments.Overlay))
                continue; // we aren't an overlay, ignore overlay instances

            var processArguments = ParseArgs(commandLine.Split(' '));
            if (processArguments.ConfigDirectory == launchArguments.ConfigDirectory)
            {
                isDuplicateProcessRunning = true;
                break;
            }
        }

        foreach (var process in processes)
            process.Dispose();

        return isDuplicateProcessRunning;
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