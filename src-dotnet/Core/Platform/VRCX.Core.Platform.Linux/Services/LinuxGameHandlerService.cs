using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;
using NLog;
using VRCX.Core.Services;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Platform.Linux.Services;

public sealed partial class LinuxGameHandlerService : IGameHandlerService, IDisposable
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly ProcessMonitorService _processMonitorService;

    public event EventHandler<bool>? OnGameRunningChanged;

    public LinuxGameHandlerService(ProcessMonitorService processMonitorService)
    {
        _processMonitorService = processMonitorService;

        processMonitorService.ProcessStarted += OnProgressStateChanged;
        processMonitorService.ProcessExited += OnProgressStateChanged;
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
                FileName = $"steam://run/438100//{HttpUtility.UrlEncode(arguments)}/",
                CreateNoWindow = true,
                UseShellExecute = true
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "Failed to launch VRChat via Steam. Falling back to registry method.");
        }

        return await LaunchGameFromRegisterAsync(arguments);
    }

    public ValueTask<bool> LaunchGameFromPathAsync(string gamePath, string arguments)
    {
        if (File.Exists(gamePath))
        {
            _logger.Error("Failed to launch VRChat from path: File not found - {GamePath}", gamePath);
            return ValueTask.FromResult(false);
        }

        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(gamePath),
                FileName = gamePath,
                Arguments = arguments,
            });

            return ValueTask.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to launch VRChat from path: {GamePath}", gamePath);
            return ValueTask.FromResult(false);
        }
    }

    private async ValueTask<bool> LaunchGameFromRegisterAsync(string arguments)
    {
        throw new NotImplementedException();
    }

    private static string? TryParseExecutablePathFromRegistryValue(string keyValue)
    {
        if (keyValue.StartsWith('"'))
            return keyValue;

        var regex = ExecutablePathRegex();
        var match = regex.Match(keyValue);
        if (!match.Success)
            return null;

        return match.Groups["ExePath"].Value;
    }

    [GeneratedRegex("""(?:^"(?<ExePath>.+?)")""")]
    private static partial Regex ExecutablePathRegex();

    #endregion

    public void Dispose()
    {
        _processMonitorService.ProcessStarted -= OnProgressStateChanged;
        _processMonitorService.ProcessExited -= OnProgressStateChanged;
    }
}