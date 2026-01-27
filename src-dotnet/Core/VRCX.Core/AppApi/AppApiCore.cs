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

public partial class AppApiCore(
    AutoAppLaunchService appLaunchService,
    LogWatcherService logWatcherService,
    ProcessMonitorService processMonitorService,
    IMainWebViewService mainWebViewService,
    ImageCacheService imageCacheService,
    StartupArgsService startupArgsService
) : WebViewInterop.App.AppApi(appLaunchService, logWatcherService, imageCacheService, startupArgsService)
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Shows the developer tools for the main browser window.
        /// </summary>
        public override void ShowDevTools()
        {
            mainWebViewService.ShowDevTools();
        }

        public override void SetVR(bool active, bool hmdOverlay, bool wristOverlay, bool menuButton, int overlayHand)
        {
            // TODO
        }

        public override async Task SetZoom(double zoomLevel)
        {
            await mainWebViewService.SetZoomLevelAsync(zoomLevel);
        }

        public override async Task<double> GetZoom()
        {
            return await mainWebViewService.GetZoomLevelAsync();
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

        public override string GetClipboard()
        {
            return "";
            // TODO
            // var clipboard = string.Empty;
            // var thread = new Thread(() => clipboard = Clipboard.GetText());
            // thread.SetApartmentState(ApartmentState.STA);
            // thread.Start();
            // thread.Join();
            // return clipboard;
        }

        public override void SetStartup(bool enabled)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (key == null)
                {
                    logger.Warn("Failed to open startup registry key");
                    return;
                }

                if (enabled)
                {
                    var path = AppDomain.CurrentDomain;
                    key.SetValue("VRCX", $"\"{path}\" --startup");
                }
                else
                {
                    key.DeleteValue("VRCX", false);
                }
            }
            catch (Exception e)
            {
                logger.Warn(e, "Failed to set startup");
            }
        }

        public override void CopyImageToClipboard(string path)
        {
            if (!File.Exists(path) ||
                (!path.EndsWith(".png") &&
                 !path.EndsWith(".jpg") &&
                 !path.EndsWith(".jpeg") &&
                 !path.EndsWith(".gif") &&
                 !path.EndsWith(".bmp") &&
                 !path.EndsWith(".webp")))
                return;

            // TODO
            // MainForm.Instance.BeginInvoke(new MethodInvoker(() =>
            // {
            //     var image = Image.FromFile(path);
            //     // Clipboard.SetImage(image);
            //     var data = new DataObject();
            //     data.SetData(DataFormats.Bitmap, image);
            //     data.SetFileDropList(new StringCollection { path });
            //     Clipboard.SetDataObject(data, true);
            // }));
        }

        public override void FlashWindow()
        {
            // TODO
            // MainForm.Instance.BeginInvoke(new MethodInvoker(() => { WinformThemer.Flash(MainForm.Instance); }));
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