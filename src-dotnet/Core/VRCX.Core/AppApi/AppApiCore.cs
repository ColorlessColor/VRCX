using System.Diagnostics;
using NLog;
using VRCX.Core.Services;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.AppApi;

public partial class AppApiCore : WebViewInterop.App.AppApi
{
    private readonly AutoAppLaunchService _appLaunchService;
    private readonly ProcessMonitorService _processMonitorService;
    private readonly IMainWebViewService _mainWebViewService;
    private readonly IClipboardService _clipboardService;
    private readonly IGameFolderProvider _gameFolderProvider;
    private readonly IGameHandlerService _gameHandlerService;
    private readonly IGamePlayPrefsService _gamePlayPrefsService;
    private readonly IFileDialogService _fileDialogService;
    private readonly IOsStartupSettingsService _startupSettingsService;
    private readonly IAppWindowService _appWindowService;
    private readonly ITrayIconService _trayIconService;
    private readonly IDesktopNotificationService _desktopNotificationService;

    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public AppApiCore(
        AutoAppLaunchService appLaunchService,
        LogWatcherService logWatcherService,
        ProcessMonitorService processMonitorService,
        ImageCacheService imageCacheService,
        StartupArgsService startupArgsService,
        IMainWebViewService mainWebViewService,
        IClipboardService clipboardService,
        IGameFolderProvider gameFolderProvider,
        IGameHandlerService gameHandlerService,
        IGamePlayPrefsService gamePlayPrefsService,
        IFileDialogService fileDialogService,
        IOsStartupSettingsService startupSettingsService,
        IAppWindowService appWindowService, 
        ITrayIconService trayIconService,
        IDesktopNotificationService desktopNotificationService) :
        base(appLaunchService, logWatcherService, imageCacheService, startupArgsService)
    {
        _appLaunchService = appLaunchService;
        _processMonitorService = processMonitorService;
        _mainWebViewService = mainWebViewService;
        _clipboardService = clipboardService;
        _gameFolderProvider = gameFolderProvider;
        _gameHandlerService = gameHandlerService;
        _gamePlayPrefsService = gamePlayPrefsService;
        _fileDialogService = fileDialogService;
        _startupSettingsService = startupSettingsService;
        _appWindowService = appWindowService;
        _trayIconService = trayIconService;
        _desktopNotificationService = desktopNotificationService;

        RegisterGameHandlerEvents();
    }

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Shows the developer tools for the main browser window.
    /// </summary>
    public override void ShowDevTools()
    {
        _mainWebViewService.ShowDevTools();
    }

    public override void SetVR(bool active, bool hmdOverlay, bool wristOverlay, bool menuButton, int overlayHand)
    {
        // TODO
    }

    public override async Task SetZoom(double zoomLevel)
    {
        await _mainWebViewService.SetZoomLevelAsync(zoomLevel);
    }

    public override async Task<double> GetZoom()
    {
        return await _mainWebViewService.GetZoomLevelAsync();
    }

    public override async Task DesktopNotification(string BoldText, string Text = "", string Image = "")
    {
        try
        {
            await _desktopNotificationService.SendDesktopNotificationAsync(BoldText, Text, Image);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error sending desktop notification");
        }
    }

    public override void RestartApplication(bool isUpgrade)
    {
        var args = new List<string>();

        if (isUpgrade)
            args.Add(VrcxLaunchArguments.IsUpgradePrefix);

        // TODO: Re-implement restart logic
        // if (startupArgsService.LaunchArguments is null)
        //     throw new InvalidOperationException("Launch arguments are null");
        //
        // if (startupArgsService.LaunchArguments.IsDebug)
        //     args.Add(VrcxLaunchArguments.IsDebugPrefix);
        //
        // if (!string.IsNullOrWhiteSpace(startupArgsService.LaunchArguments.ConfigDirectory))
        //     args.Add($"{VrcxLaunchArguments.ConfigDirectoryPrefix}={startupArgsService.LaunchArguments.ConfigDirectory}");
        //
        // if (!string.IsNullOrWhiteSpace(startupArgsService.LaunchArguments.ProxyUrl))
        //     args.Add($"{VrcxLaunchArguments.ProxyUrlPrefix}={startupArgsService.LaunchArguments.ProxyUrl}");
        //
        // var vrcxProcess = new Process
        // {
        //     StartInfo = new ProcessStartInfo
        //     {
        //         FileName = Path.Join(AppPathService.BaseDirectory, "VRCX.exe"),
        //         Arguments = string.Join(' ', args),
        //         UseShellExecute = true,
        //         WorkingDirectory = AppPathService.BaseDirectory
        //     }
        // };
        // vrcxProcess.Start();
        // Environment.Exit(0);
    }

    public override bool CheckForUpdateExe()
    {
        return File.Exists(Path.Join(AppPathService.AppDataDirectory, "update.exe"));
    }

    public override void ExecuteVrOverlayFunction(string function, string json)
    {
        // TODO
        // var message = new OverlayMessage
        // {
        //     Type = OverlayMessageType.JsFunctionCall,
        //     FunctionName = function,
        //     Data = json
        // };
        // OverlayServer.Instance.SendMessage(message);
    }

    public override async Task FocusWindow()
    {
        await _appWindowService.FocusMainWindowAsync();
    }

    public override async Task ChangeTheme(int value)
    {
        await _appWindowService.ChangeAppThemeAsync((AppTheme)value);
    }

    public override async Task<string> GetClipboard()
    {
        return await _clipboardService.GetClipboardAsString();
    }

    public override async Task SetStartup(bool enabled)
    {
        if (enabled)
        {
            await _startupSettingsService.EnableAutoLaunchAsync();
        }
        else
        {
            await _startupSettingsService.DisableAutoLaunchAsync();
        }
    }

    public override async Task CopyImageToClipboard(string path)
    {
        if (!File.Exists(path) ||
            (!path.EndsWith(".png") &&
             !path.EndsWith(".jpg") &&
             !path.EndsWith(".jpeg") &&
             !path.EndsWith(".gif") &&
             !path.EndsWith(".bmp") &&
             !path.EndsWith(".webp")))
            return;

        await _clipboardService.SetBitmapAsync(path);
    }

    public override async Task SetUserAgent()
    {
        await _mainWebViewService.SetUserAgentAsync(AppBuildInfoService.Version);
    }

    public override async Task SetTrayIconNotification(bool notify)
    {
        await _trayIconService.SetTrayIconNotificationAsync(notify);
    }

    public override void OpenCalendarFile(string icsContent)
    {
        // validate content
        if (!icsContent.StartsWith("BEGIN:VCALENDAR") ||
            !icsContent.EndsWith("END:VCALENDAR"))
            throw new Exception("Invalid calendar file");

        try
        {
            var tempPath = Path.Combine(AppPathService.AppDataDirectory, "event.ics");
            File.WriteAllText(tempPath, icsContent);
            Process.Start(new ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true
            })?.Dispose();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to open calendar file");
        }
    }
}