using NLog;
using VRCX.Core.Services;
using VRCX.Core.Services.Platform;
using VRCX.Core.Utils;

namespace VRCX.Core.Platform.Windows.Services;

public sealed class WindowsGameRunningStatusService : IGameRunningStatusService, IDisposable
{
    private readonly ProcessMonitorService _processMonitorService;

    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private const string WindowsSteamVrProcessName = "vrmonitor";

    public event EventHandler<bool>? OnGameRunningChanged;

    public WindowsGameRunningStatusService(ProcessMonitorService processMonitorService)
    {
        _processMonitorService = processMonitorService;

        _processMonitorService.AddProcess(VRChatUtils.VRChatProcessName);
        _processMonitorService.AddProcess(WindowsSteamVrProcessName);

        _processMonitorService.ProcessStarted += OnProcessStateChanged;
        _processMonitorService.ProcessExited += OnProcessStateChanged;
    }

    private void OnProcessStateChanged(MonitoredProcess process)
    {
        _logger.Debug("Updateing game running state due to process {ProcessName} {EventType}",
            process.ProcessName,
            process.IsRunning ? "started" : "exited");
        OnGameRunningChanged?.Invoke(this, IsGameRunningInternal());
    }

    private bool IsGameRunningInternal() => _processMonitorService.IsProcessRunning(VRChatUtils.VRChatProcessName);

    public bool IsGameRunning()
    {
        return IsGameRunningInternal();
    }

    public bool IsSteamVRRunning()
    {
        return _processMonitorService.IsProcessRunning(WindowsSteamVrProcessName);
    }

    public void Dispose()
    {
        _processMonitorService.ProcessStarted -= OnProcessStateChanged;
        _processMonitorService.ProcessExited -= OnProcessStateChanged;
    }
}