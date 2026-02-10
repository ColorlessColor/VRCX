using Serilog;
using VRCX.Core.Services;
using VRCX.Core.Services.Platform;
using VRCX.Core.Utils;

namespace VRCX.Core.Platform.Linux.Services;

public sealed class LinuxGameRunningStatusService : IGameRunningStatusService, IDisposable
{
    private readonly ILogger _logger = Log.ForContext<LinuxGameRunningStatusService>();

    private readonly ProcessMonitorService _processMonitorService;

    private readonly string[] _steamVrProcessNameList =
    [
        "vrmonitor", "monado-service", "wivrn-server"
    ];

    public event EventHandler<bool>? OnGameRunningChanged;

    public LinuxGameRunningStatusService(ProcessMonitorService processMonitorService)
    {
        _processMonitorService = processMonitorService;

        _processMonitorService.AddProcess(VRChatUtils.VRChatProcessName);
        foreach (var name in _steamVrProcessNameList)
        {
            _processMonitorService.AddProcess(name);
        }

        _processMonitorService.ProcessStarted += OnProcessStateChanged;
        _processMonitorService.ProcessExited += OnProcessStateChanged;
    }

    private void OnProcessStateChanged(MonitoredProcess process)
    {
        _logger.Debug("Updateing game running state due to process {ProcessName} {EventType}",
            process.ProcessName,
            process.IsRunning ? "started" : "exited");
        OnGameRunningChanged?.Invoke(this, IsGameRunning());
    }

    public bool IsGameRunning() => _processMonitorService.IsProcessRunning(VRChatUtils.VRChatProcessName);

    public bool IsSteamVRRunning()
    {
        return _steamVrProcessNameList.Any(name =>
            _processMonitorService.IsProcessRunning(name));
    }

    public void Dispose()
    {
        _processMonitorService.ProcessStarted -= OnProcessStateChanged;
        _processMonitorService.ProcessExited -= OnProcessStateChanged;
    }
}