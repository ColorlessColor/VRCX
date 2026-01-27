using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using VRCX.Core.Extensions;
using VRCX.Core.Services;

namespace VRCX.Core.AppApi;

public partial class AppApiCore
{
    public override void OnProcessStateChanged(MonitoredProcess monitoredProcess)
    {
        if (!monitoredProcess.HasName("VRChat") && !monitoredProcess.HasName("vrserver"))
            return;

        CheckGameRunning();
    }

    public override void CheckGameRunning()
    {
        var isGameRunning = false;
        var isSteamVRRunning = false;

        if (processMonitorService.IsProcessRunning("VRChat"))
            isGameRunning = true;

        if (processMonitorService.IsProcessRunning("vrserver"))
            isSteamVRRunning = true;

        mainWebViewService.ExecuteScriptAsync(
            "window?.$pinia?.game.updateIsGameRunning",
            isGameRunning,
            isSteamVRRunning
        );
    }

    public override bool IsGameRunning()
    {
        // unused
        return processMonitorService.IsProcessRunning("VRChat");
    }

    public override bool IsSteamVRRunning()
    {
        // unused
        return processMonitorService.IsSteamVrRunning();
    }

    public override int QuitGame()
    {
        var processes = Process.GetProcessesByName("VRChat");
        if (processes.Length == 1)
            processes[0].Kill();
        foreach (var process in processes)
            process.Dispose();

        return processes.Length;
    }

    public override bool StartGame(string arguments)
    {
        // try stream first
        try
        {
            using var key = Registry.ClassesRoot.OpenSubKey(@"steam\shell\open\command");
            // "C:\Program Files (x86)\Steam\steam.exe" -- "%1"
            var match = Regex.Match(key.GetValue(string.Empty) as string, "^\"(.+?)\\\\steam.exe\"");
            if (match.Success)
            {
                var path = match.Groups[1].Value;
                // var _arguments = Uri.EscapeDataString(arguments);
                Process.Start(new ProcessStartInfo
                {
                    WorkingDirectory = path,
                    FileName = $"{path}\\steam.exe",
                    UseShellExecute = false,
                    Arguments = $"-applaunch 438100 {arguments}"
                })?.Dispose();
                return true;
            }
        }
        catch
        {
            logger.Warn("Failed to start VRChat from Steam");
        }

        // fallback
        try
        {
            using var key = Registry.ClassesRoot.OpenSubKey(@"VRChat\shell\open\command");
            // "C:\Program Files (x86)\Steam\steamapps\common\VRChat\launch.exe" "%1" %*
            var match = Regex.Match(key.GetValue(string.Empty) as string,
                "(?!\")(.+?\\\\VRChat.*)(!?\\\\launch.exe\")");
            if (match.Success)
            {
                var path = match.Groups[1].Value;
                return StartGameFromPath(path, arguments);
            }
        }
        catch
        {
            logger.Warn("Failed to start VRChat from registry");
        }

        return false;
    }

    public override bool StartGameFromPath(string path, string arguments)
    {
        if (!path.EndsWith(".exe"))
            path = Path.Join(path, "launch.exe");

        if (!path.EndsWith("launch.exe") || !File.Exists(path))
            return false;

        Process.Start(new ProcessStartInfo
        {
            WorkingDirectory = Path.GetDirectoryName(path),
            FileName = path,
            UseShellExecute = false,
            Arguments = arguments
        })?.Dispose();
        return true;
    }
}