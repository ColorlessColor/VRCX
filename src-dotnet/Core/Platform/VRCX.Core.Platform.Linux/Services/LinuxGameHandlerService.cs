using System.Diagnostics;
using NLog;
using VRCX.Core.Services;
using VRCX.Core.Services.Platform;
using VRCX.Core.Platform.Linux.Utils;
using VRCX.Core.Utils;

namespace VRCX.Core.Platform.Linux.Services;

public sealed class LinuxGameHandlerService : IGameHandlerService, IDisposable
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly ProcessMonitorService _processMonitorService;

    private readonly string? _steamPath;

    public event EventHandler<bool>? OnGameRunningChanged;

    public LinuxGameHandlerService(ProcessMonitorService processMonitorService)
    {
        _processMonitorService = processMonitorService;

        processMonitorService.ProcessStarted += OnProgressStateChanged;
        processMonitorService.ProcessExited += OnProgressStateChanged;

        switch (SteamPathUtils.GetSteamPath(out var steamPath))
        {
            case SteamPathUtils.SteamPathType.HostInstalledSteam:
                _logger.Info("Host installed Steam detected.");
                break;
            case SteamPathUtils.SteamPathType.FlatpakSteam:
                _logger.Info("Flatpak Steam detected.");
                break;
            case SteamPathUtils.SteamPathType.LegacySteam:
                _logger.Info("Legacy Steam path detected.");
                break;
            case SteamPathUtils.SteamPathType.NoValidSteam:
            default:
                _logger.Error("No valid Steam library found.");
                break;
        }

        _steamPath = steamPath;
    }

    private void OnProgressStateChanged(MonitoredProcess process)
    {
        _logger.Debug("Updateing game running state due to process {ProcessName} {EventType}",
            process.ProcessName,
            process.IsRunning ? "started" : "exited");
        OnGameRunningChanged?.Invoke(this, IsGameRunningCore());
    }

    private bool IsGameRunningCore() => _processMonitorService.IsProcessRunning("VRChat");

    public ValueTask<bool> IsGameRunningAsync()
    {
        return new ValueTask<bool>(IsGameRunningCore());
    }

    public ValueTask<int> QuitGameAsync()
    {
        var processes = Process.GetProcessesByName("VRChat");
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
            _logger.Warn(ex, "Failed to launch VRChat via Steam. Attempting to launch via Steam path.");
        }

        return await LaunchGameFromSteamPathAsync(arguments);
    }

    private ValueTask<bool> LaunchGameFromSteamPathAsync(string arguments)
    {
        try
        {
            if (string.IsNullOrEmpty(_steamPath))
            {
                _logger.Error("Failed to launch VRChat via Steam path: Steam path could not be determined.");
                return ValueTask.FromResult(false);
            }

            var steamExecutable = Path.Join(_steamPath, "steam.sh");
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
            _logger.Error(ex, "Failed to launch VRChat via Steam path.");
            return ValueTask.FromResult(false);
        }
    }

    public ValueTask<bool> LaunchGameFromPathAsync(string gamePath, string arguments)
    {
        // This method is not used
        _logger.Error("Failed to launch VRChat from path: Platform not supported.");
        return ValueTask.FromResult(false);
    }

    #endregion

    public void Dispose()
    {
        _processMonitorService.ProcessStarted -= OnProgressStateChanged;
        _processMonitorService.ProcessExited -= OnProgressStateChanged;
    }
}