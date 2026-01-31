using System.Diagnostics;
using NLog;
using VRCX.Core.Extensions;
using VRCX.Core.Utils;

namespace VRCX.Core.Services;

public sealed class AutoAppLaunchService : IDisposable
{
    private readonly ProcessMonitorService _processMonitorService;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    
    public const string VRChatProcessName = "VRChat";

    public bool Enabled = false;

    /// <summary> Whether or not to kill child processes when VRChat closes. </summary>
    public bool KillChildrenOnExit = true;

    public bool RunProcessOnce = true;
    public readonly string AppShortcutDirectory;
    public readonly string AppShortcutDesktop;
    public readonly string AppShortcutVR;

    private readonly DateTime _appStartTime = DateTime.Now;

    private readonly Lock _processLock = new();
    private readonly List<Process> _processesStarted = [];

    public AutoAppLaunchService(ProcessMonitorService processMonitorService)
    {
        _processMonitorService = processMonitorService;
        AppShortcutDirectory = Path.Join(AppPathService.AppDataDirectory, "startup");
        AppShortcutDesktop = Path.Join(AppShortcutDirectory, "desktop");
        AppShortcutVR = Path.Join(AppShortcutDirectory, "vr");

        Directory.CreateDirectory(AppShortcutDirectory);
        Directory.CreateDirectory(AppShortcutDesktop);
        Directory.CreateDirectory(AppShortcutVR);

        processMonitorService.ProcessStarted += OnProcessStarted;
        processMonitorService.ProcessExited += OnProcessExited;
    }

    private void OnProcessExited(MonitoredProcess monitoredProcess)
    {
        if (!monitoredProcess.HasName(VRChatProcessName))
            return;

        lock (_processLock)
        {
            if (KillChildrenOnExit)
            {
                KillChildProcesses();
            }

            _processesStarted.Clear();
        }
    }

    private void OnProcessStarted(MonitoredProcess monitoredProcess)
    {
        if (!Enabled || !monitoredProcess.HasName(VRChatProcessName) || monitoredProcess.Process?.StartTime < _appStartTime)
            return;

        lock (_processLock)
        {
            if (KillChildrenOnExit)
                KillChildProcesses();

            var shortcutFiles = WindowsShortcutUtils.FindShortcutFiles(AppShortcutDirectory);
            shortcutFiles.AddRange(WindowsShortcutUtils.FindShortcutFiles(_processMonitorService.IsSteamVrRunning()
                ? AppShortcutVR
                : AppShortcutDesktop));

            foreach (var file in shortcutFiles)
            {
                if (RunProcessOnce && IsProcessRunning(file))
                    continue;

                if (IsChildProcessRunning(file))
                    continue;

                StartChildProcess(file);
            }
        }
    }

    /// <summary>
    /// Kills all running child processes.
    /// </summary>
    private void KillChildProcesses()
    {
        Parallel.ForEach(_processesStarted, process =>
        {
            if (process.HasExited)
                return;

            try
            {
                process.Kill(true);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Error killing child process: ({ProcessId}) {PrcoessName}", process.Id,
                    process.ProcessName);
            }
        });
    }

    /// <summary>
    /// Starts a new child process.
    /// </summary>
    /// <param name="path">The path.</param>
    private void StartChildProcess(string path)
    {
        try
        {
            var process = new Process();
            process.StartInfo = new ProcessStartInfo(path)
            {
                UseShellExecute = true
            };

            process.Start();

            _processesStarted.Add(process);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error starting child process: {PathToProcess}", path);
        }
    }

    /// <summary>
    /// Checks to see if a given file matches a current running child process.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>
    ///   <c>true</c> if child process running; otherwise, <c>false</c>.
    /// </returns>
    private bool IsChildProcessRunning(string path)
    {
        return _processesStarted
            .Where(p => !p.HasExited)
            .Any(p =>
                p.MainModule != null &&
                !string.IsNullOrWhiteSpace(p.MainModule.FileName) &&
                PathUtils.NormalizePath(p.MainModule.FileName) == PathUtils.NormalizePath(path)
            );
    }

    private bool IsProcessRunning(string filePath)
    {
        try
        {
            var processName = Path.GetFileNameWithoutExtension(filePath);
            var processes = Process.GetProcessesByName(processName);
            var isProcessRunning = processes.Length != 0;
            foreach (var process in processes)
                process.Dispose();

            return isProcessRunning;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error checking if process is running: {PathToCheck}", filePath);
            return false;
        }
    }

    public void Dispose()
    {
        Enabled = false;
    }
}