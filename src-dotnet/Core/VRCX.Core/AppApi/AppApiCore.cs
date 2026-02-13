using Serilog;
using VRCX.Core.Models.OverlayWebSocket;
using VRCX.Core.OverlayClient.XsOverlay.Services;
using VRCX.Core.Services;
using VRCX.Core.Services.AppUpdate;
using VRCX.Core.Services.Ipc;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.AppApi;

public partial class AppApiCore : WebViewInterop.App.AppApi
{
    private readonly AutoAppLaunchService _appLaunchService;
    private readonly ProcessMonitorService _processMonitorService;
    private readonly AppUpdateService _appUpdateService;
    private readonly OverlayWebSocketService _overlayWebSocketService;
    private readonly IMainWebViewService _mainWebViewService;
    private readonly IClipboardService _clipboardService;
    private readonly IGameFolderProvider _gameFolderProvider;
    private readonly IGameHandlerService _gameHandlerService;
    private readonly IGameRunningStatusService _gameRunningStatusService;
    private readonly IGamePlayPrefsService _gamePlayPrefsService;
    private readonly IFileDialogService _fileDialogService;
    private readonly IOsStartupSettingsService _startupSettingsService;
    private readonly IAppWindowService _appWindowService;
    private readonly ITrayIconService _trayIconService;
    private readonly IDesktopNotificationService _desktopNotificationService;
    private readonly IPlatformLifetimeService _platformLifetimeService;
    private readonly IPlatformLauncherService _platformLauncherService;

    private readonly ILogger _logger = Log.ForContext<AppApiCore>();

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
        IGameRunningStatusService gameRunningStatusService,
        IGamePlayPrefsService gamePlayPrefsService,
        IFileDialogService fileDialogService,
        IOsStartupSettingsService startupSettingsService,
        IAppWindowService appWindowService,
        ITrayIconService trayIconService,
        IDesktopNotificationService desktopNotificationService,
        IPlatformLifetimeService platformLifetimeService,
        IPlatformLauncherService platformLauncherService,
        INotifyWebLoadedService notifyWebLoadedService,
        AppUpdateService appUpdateService,
        OverlayWebSocketService overlayWebSocketService,
        IpcServerService ipcServerService,
        XsOverlayClientService xsOverlayClientService) :
        base(
            appLaunchService, logWatcherService, imageCacheService, startupArgsService, appUpdateService,
            platformLauncherService, notifyWebLoadedService, ipcServerService, xsOverlayClientService
        )
    {
        _appLaunchService = appLaunchService;
        _processMonitorService = processMonitorService;
        _mainWebViewService = mainWebViewService;
        _clipboardService = clipboardService;
        _gameFolderProvider = gameFolderProvider;
        _gameHandlerService = gameHandlerService;
        _gameRunningStatusService = gameRunningStatusService;
        _gamePlayPrefsService = gamePlayPrefsService;
        _fileDialogService = fileDialogService;
        _startupSettingsService = startupSettingsService;
        _appWindowService = appWindowService;
        _trayIconService = trayIconService;
        _desktopNotificationService = desktopNotificationService;
        _appUpdateService = appUpdateService;
        _overlayWebSocketService = overlayWebSocketService;
        _platformLifetimeService = platformLifetimeService;
        _platformLauncherService = platformLauncherService;

        RegisterGameHandlerEvents();
    }

    /// <summary>
    /// Shows the developer tools for the main browser window.
    /// </summary>
    public override void ShowDevTools()
    {
        _mainWebViewService.ShowDevTools();
    }

    public override void SetVR(bool active, bool hmdOverlay, bool wristOverlay, bool menuButton, int overlayHand)
    {
        var updateVars = new OverlayVars
        {
            Active = active,
            HmdOverlay = hmdOverlay,
            WristOverlay = wristOverlay,
            MenuButton = menuButton,
            OverlayHand = overlayHand
        };

        _overlayWebSocketService.UpdateVars(updateVars);
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
            _logger.Error(ex, "Error sending desktop notification");
        }
    }

    public override async Task RestartApplication(bool isUpgrade)
    {
        await _platformLifetimeService.InvokeRestartAsync();
    }

    public override async Task<bool> CheckForUpdateExe()
    {
        return await _appUpdateService.GetInProgressUpdateTargetVersionAsync() != null;
    }

    public override void ExecuteVrOverlayFunction(string function, string json)
    {
        _overlayWebSocketService.SendMessage(new OverlayMessage
        {
            Type = OverlayMessageType.JsFunctionCall,
            FunctionName = function,
            Data = json
        });
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

    public override async Task OpenCalendarFile(string icsContent)
    {
        // validate content
        if (!icsContent.StartsWith("BEGIN:VCALENDAR") ||
            !icsContent.EndsWith("END:VCALENDAR"))
            throw new Exception("Invalid calendar file");

        try
        {
            var tempPath = Path.Combine(AppPathService.AppDataDirectory, "event.ics");
            await File.WriteAllTextAsync(tempPath, icsContent);
            await _platformLauncherService.LaunchFileAsync(tempPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to open calendar file");
        }
    }
}