using System.Diagnostics;
using Serilog;
using VRCX.Core.Services;
using VRCX.LegacyApp.WinFormsCef.CoreGlue.LegacySingleton;

namespace VRCX.LegacyApp.WinFormsCef.OverlayWebSocket;

public class OverlayManager
{
    private static readonly ILogger Logger = Log.ForContext<OverlayManager>();
    
    private static Process? _process;

    public static void StartOverlay()
    {
        if (OverlayServer.Instance.IsConnected() ||
            _process != null && !_process.HasExited)
        {
            Logger.Error("Overlay server already started");
            return;
        }

        StartOverlayProcess();
    }
    
    private static void StartOverlayProcess()
    {
        if (Environment.ProcessPath == null)
        {
            Logger.Error("Cannot start Overlay process without a process path");
            return;
        }
        
        var args = new List<string>();
        args.Add(VrcxLaunchArguments.Overlay);
        if (Program.LaunchDebug)
            args.Add(VrcxLaunchArguments.IsDebugPrefix);
        if (StartupArgs.Instance.LaunchArguments.ConfigDirectory != null)
            args.Add($"{VrcxLaunchArguments.ConfigDirectoryPrefix}={StartupArgs.Instance.LaunchArguments.ConfigDirectory}");

        var startInfo = new ProcessStartInfo
        {
            FileName = Environment.ProcessPath,
            Arguments = string.Join(' ', args),
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Program.BaseDirectory
        };
        _process = Process.Start(startInfo);
        Logger.Information("Overlay process started");
    }
}