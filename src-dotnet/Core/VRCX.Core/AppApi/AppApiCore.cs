using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
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
        IOsStartupSettingsService startupSettingsService) :
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

    public override void DesktopNotification(string BoldText, string Text = "", string Image = "")
    {
        // TODO

        // try
        // {
        //     ToastContentBuilder builder = new ToastContentBuilder();
        //
        //     if (Uri.TryCreate(Image, UriKind.Absolute, out Uri uri))
        //         builder.AddAppLogoOverride(uri);
        //
        //     if (!string.IsNullOrEmpty(BoldText))
        //         builder.AddText(BoldText);
        //
        //     if (!string.IsNullOrEmpty(Text))
        //         builder.AddText(Text);
        //
        //     builder.Show();
        // }
        // catch (System.AccessViolationException ex)
        // {
        //     logger.Warn(ex, "Unable to send desktop notification");
        // }
        // catch (Exception ex)
        // {
        //     logger.Error(ex, "Unknown error when sending desktop notification");
        // }
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

    public override void FocusWindow()
    {
        // TODO
    }

    public override void ChangeTheme(int value)
    {
        // TODO
        // WinformThemer.SetGlobalTheme(value);
    }

    public override void DoFunny()
    {
        // TODO
        // WinformThemer.DoFunny();
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

    public override void SetUserAgent()
    {
        // TODO
        // using var client = MainForm.Instance.Browser.GetDevToolsClient();
        // _ = client.Network.SetUserAgentOverrideAsync(Program.Version);
    }

    public override void SetTrayIconNotification(bool notify)
    {
        // TODO
        // MainForm.Instance.BeginInvoke(new MethodInvoker(() => { MainForm.Instance.SetTrayIconNotification(notify); }));
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