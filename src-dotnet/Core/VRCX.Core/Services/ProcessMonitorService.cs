using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using NLog;

namespace VRCX.Core.Services;

public sealed class ProcessMonitorService : IDisposable
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly Dictionary<string, MonitoredProcess> _monitoredProcesses = new();
    private readonly Timer _monitorProcessTimer;

    public ProcessMonitorService()
    {
        _monitorProcessTimer = new Timer();
        _monitorProcessTimer.Interval = 1000;
        _monitorProcessTimer.Elapsed += MonitorProcessTimer_Elapsed;

        // TODO: debug, remove comment later
        // Instance.ProcessStarted += Program.AppApiInstance.OnProcessStateChanged;
        // Instance.ProcessExited += Program.AppApiInstance.OnProcessStateChanged;
    }

    /// <summary>
    ///     Raised when a monitored process is started.
    /// </summary>
    public event Action<MonitoredProcess>? ProcessStarted;

    /// <summary>
    ///     Raised when a monitored process is exited.
    /// </summary>
    public event Action<MonitoredProcess>? ProcessExited;

    public void Start()
    {
        AddProcess("vrchat");
        AddProcess("vrserver");
        _monitorProcessTimer.Start();
    }

    public void Dispose()
    {
        _monitorProcessTimer.Dispose();
        _monitoredProcesses.Values.ToList().ForEach(x => x.ProcessExited());
    }

    private void MonitorProcessTimer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        var processesNeedingUpdate = new List<MonitoredProcess>();

        // Check if any of the monitored processes have been opened or closed.
        foreach (var keyValuePair in _monitoredProcesses)
        {
            var monitoredProcess = keyValuePair.Value;

            if (monitoredProcess.IsRunning)
            {
                if (monitoredProcess.Process == null || WinApi.HasProcessExited(monitoredProcess.Process.Id))
                {
                    monitoredProcess.ProcessExited();
                    ProcessExited?.Invoke(monitoredProcess);
                    _logger.Info(
                        $"Monitored process {monitoredProcess.ProcessName} (PID: {(monitoredProcess.Process?.Id.ToString() ?? "null")}) exited.");
                }
            }
            else
            {
                processesNeedingUpdate.Add(monitoredProcess);
            }
        }

        // We do it this way so we're not constantly polling for processes if we don't actually need to (aka, all processes are already accounted for).
        if (processesNeedingUpdate.Count == 0)
            return;

        var processes = Process.GetProcesses();
        foreach (var monitoredProcess in processesNeedingUpdate)
        {
            var process = processes.FirstOrDefault(p =>
                string.Equals(p.ProcessName, monitoredProcess.ProcessName, StringComparison.OrdinalIgnoreCase));

            // We are also checking to see if the process is exiting before adding it, otherwise we'll keep adding it and then removing it constantly in an endless loop.
            if (process == null || WinApi.HasProcessExited(process.Id))
                continue;

            monitoredProcess.ProcessStarted(process);
            ProcessStarted?.Invoke(monitoredProcess);
            _logger.Info($"Monitored process {monitoredProcess.ProcessName} (PID: {process.Id}) started.");
        }
    }

    /// <summary>
    ///     Checks if a process if currently being monitored and if it is running.
    /// </summary>
    /// <param name="processName">The name of the process to check for.</param>
    /// <param name="ensureCheck">If true, will manually check if the given process is running should the the monitored process not be initialized yet.</param>
    /// <returns>Whether the given process is monitored and currently running.</returns>
    public bool IsProcessRunning(string processName, bool ensureCheck = false)
    {
        processName = processName.ToLower();
        if (!_monitoredProcesses.TryGetValue(processName, out var process))
            return false;

        if (ensureCheck && process.Process == null)
        {
            var processes = Process.GetProcessesByName(processName);
            var isProcessRunning = processes.Length > 0;
            foreach (var proc in processes)
                proc.Dispose();

            return isProcessRunning;
        }

        return process.IsRunning;
    }

    /// <summary>
    ///     Adds a process to be monitored.
    /// </summary>
    /// <param name="process"></param>
    public void AddProcess(Process process)
    {
        var processName = process.ProcessName.ToLower();
        if (_monitoredProcesses.ContainsKey(processName))
            return;

        _monitoredProcesses.Add(processName, new MonitoredProcess(process));
        _logger.Debug($"Added process {processName} to process monitor.");
    }

    /// <summary>
    ///     Adds a process to be monitored.
    /// </summary>
    /// <param name="processName"></param>
    public void AddProcess(string processName)
    {
        processName = processName.ToLower();
        if (_monitoredProcesses.ContainsKey(processName))
            return;

        _monitoredProcesses.Add(processName, new MonitoredProcess(processName));
        _logger.Debug($"Added process {processName} to process monitor.");
    }

    /// <summary>
    ///     Removes a process from being monitored.
    /// </summary>
    /// <param name="processName"></param>
    public void RemoveProcess(string processName)
    {
        processName = processName.ToLower();
        _monitoredProcesses.Remove(processName);
        _logger.Debug($"Removed process {processName} from process monitor.");
    }
}