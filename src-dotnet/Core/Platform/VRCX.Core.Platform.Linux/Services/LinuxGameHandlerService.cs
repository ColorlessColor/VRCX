using System.Diagnostics;
using Serilog;
using VRCX.Core.Services.Platform;
using VRCX.Core.Utils;

namespace VRCX.Core.Platform.Linux.Services;

public sealed class LinuxGameHandlerService(LinuxSteamPathService steamPathService) : IGameHandlerService
{
    private readonly ILogger _logger = Log.ForContext<LinuxGameHandlerService>();

    public ValueTask<int> QuitGameAsync()
    {
        var processes = Process.GetProcessesByName(VRChatUtils.VRChatProcessName);
        if (processes.Length == 1)
            processes[0].Kill();

        foreach (var process in processes)
            process.Dispose();

        return ValueTask.FromResult(processes.Length);
    }

    #region Launch Game

    public async ValueTask<bool> LaunchGameAsync(string arguments)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "steam",
                Arguments = $"-applaunch {VRChatUtils.VRChatSteamAppid} {arguments}",
                UseShellExecute = false,
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to launch VRChat via Steam. Attempting to launch via Steam path");
        }

        return await LaunchGameFromSteamPathAsync(arguments);
    }

    private ValueTask<bool> LaunchGameFromSteamPathAsync(string arguments)
    {
        try
        {
            if (string.IsNullOrEmpty(steamPathService.SteamPath))
            {
                _logger.Error("Failed to launch VRChat via Steam path: Steam path could not be determined");
                return ValueTask.FromResult(false);
            }

            var steamExecutable = Path.Join(steamPathService.SteamPath, "steam.sh");
            if (!File.Exists(steamExecutable))
            {
                _logger.Error(
                    "Failed to launch VRChat via Steam path: Steam executable not exists: {SteamExecutablePath}",
                    steamExecutable);
                return ValueTask.FromResult(false);
            }

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = steamExecutable,
                Arguments = $"-applaunch {VRChatUtils.VRChatSteamAppid} {arguments}",
                UseShellExecute = false,
            });

            return ValueTask.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to launch VRChat via Steam path");
            return ValueTask.FromResult(false);
        }
    }

    public ValueTask<bool> LaunchGameFromPathAsync(string gamePath, string arguments)
    {
        // This method is not used
        _logger.Error("Failed to launch VRChat from path: Platform not supported");
        return ValueTask.FromResult(false);
    }

    #endregion
}