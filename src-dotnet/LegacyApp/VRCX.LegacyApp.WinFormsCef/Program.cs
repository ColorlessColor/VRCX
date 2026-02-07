using System.Data.SQLite;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using VRCX.Core;
using VRCX.Core.Extensions;
using VRCX.Core.Services;
using VRCX.Core.Platform.Windows.Extensions;
using VRCX.LegacyApp.WinFormsCef.Cef;
using VRCX.LegacyApp.WinFormsCef.CoreGlue.Extensions;
using VRCX.LegacyApp.WinFormsCef.CoreGlue.LegacySingleton;
using VRCX.LegacyApp.WinFormsCef.Overlay.Cef;

namespace VRCX.LegacyApp.WinFormsCef
{
    public static class Program
    {
        public static string BaseDirectory => AppPathService.BaseDirectory;
        public static string AppDataDirectory => AppPathService.AppDataDirectory;
        public static string ConfigLocation => AppPathService.ConfigLocation;
        public static string Version => AppBuildInfoService.Version;
        public static bool LaunchDebug => AppDebugService.InDebugMode;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [STAThread]
        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
        private static void Main(string[] args)
        {
            BrowserSubprocess.Start();
            if (Wine.GetIfWine())
            {
                MessageBox.Show(
                    "VRCX Cef has detected Wine.\nPlease switch to our native Electron build for Linux.",
                    "Wine Detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            try
            {
                Run(args);
            }

            #region Handle CEF Explosion

            catch (FileNotFoundException e)
            {
                logger.Error(e, "Handled Exception, Missing file found in Handle Cef Explosion.");

                var result = MessageBox.Show(
                    "VRCX has encountered an error with the CefSharp backend,\nthis is typically caused by missing files or dependencies.\nWould you like to try autofix by automatically installing vc_redist?.",
                    "VRCX CefSharp not found.", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                switch (result)
                {
                    case DialogResult.Yes:
                        logger.Fatal("Handled Exception, user selected auto install of vc_redist.");
                        Update.DownloadInstallRedist().GetAwaiter().GetResult();
                        MessageBox.Show(
                            "vc_redist has finished installing, if the issue persists upon next restart, please reinstall VRCX From GitHub,\nVRCX Will now restart.",
                            "vc_redist installation complete", MessageBoxButtons.OK);
                        Thread.Sleep(5000);
                        RestartApplication(false);
                        break;

                    case DialogResult.No:
                        logger.Fatal("Handled Exception, user chose manual.");
                        MessageBox.Show(
                            "VRCX will now close, try reinstalling VRCX using the setup from Github as a potential fix.",
                            "VRCX CefSharp not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Thread.Sleep(5000);
                        Environment.Exit(0);
                        break;
                }
            }

            #endregion

            #region Handle Database Error

            catch (SQLiteException e)
            {
                logger.Fatal(e, "Unhandled SQLite Exception, closing.");
                var messageBoxResult = MessageBox.Show(
                    "A fatal database error has occured.\n" +
                    "Please try to repair your database by following the steps in the provided repair guide, or alternatively rename your \"%AppData%\\VRCX\" folder to reset VRCX. " +
                    "If the issue still persists after following the repair guide please join the Discord (https://vrcx.app/discord) for further assistance. " +
                    "Would you like to open the webpage for database repair steps?\n" +
                    e, "Database error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (messageBoxResult == DialogResult.Yes)
                {
                    OpenLink("https://github.com/vrcx-team/VRCX/wiki#how-to-repair-vrcx-database");
                }
            }

            #endregion

            catch (Exception e)
            {
                var cpuError = WinApi.GetCpuErrorMessage();
                if (cpuError != null)
                {
                    var messageBoxResult = MessageBox.Show(cpuError.Value.Item1, "Potentially Faulty CPU Detected",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    if (messageBoxResult == DialogResult.Yes)
                    {
                        OpenLink(cpuError.Value.Item2);
                    }
                }

                logger.Fatal(e, "Unhandled Exception, program dying");
                MessageBox.Show(e.ToString(), "PLEASE REPORT IN https://vrcx.app/discord", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        private static void Run(string[] args)
        {
            var serviceProvider = BuildServices();
            serviceProvider.RunApp(args, () =>
                {
                    // Main App Startup
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    CefService.Instance.Init();
                    Application.Run(new MainForm());
                    CefService.Instance.Exit();
                }, // Overlay Startup
                OverlayProgram.OverlayMain);
        }

        private static ServiceProvider BuildServices()
        {
            var services = new ServiceCollection();

            services.AddCoreServices();
            services.AddWindowsPlatformServices();
            services.AddWinFormsCefAppServices();

            return services.BuildServiceProvider();
        }

        #region Helper Methods

        private static void RestartApplication(bool isUpgrade)
        {
            var args = new List<string>();

            if (isUpgrade)
                args.Add(VrcxLaunchArguments.IsUpgradePrefix);

            if (StartupArgs.Instance.LaunchArguments.IsDebug)
                args.Add(VrcxLaunchArguments.IsDebugPrefix);

            if (!string.IsNullOrWhiteSpace(StartupArgs.Instance.LaunchArguments.ConfigDirectory))
                args.Add(
                    $"{VrcxLaunchArguments.ConfigDirectoryPrefix}={StartupArgs.Instance.LaunchArguments.ConfigDirectory}");

            if (!string.IsNullOrWhiteSpace(StartupArgs.Instance.LaunchArguments.ProxyUrl))
                args.Add($"{VrcxLaunchArguments.ProxyUrlPrefix}={StartupArgs.Instance.LaunchArguments.ProxyUrl}");

            var vrcxProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Join(BaseDirectory, "VRCX.exe"),
                    Arguments = string.Join(' ', args),
                    UseShellExecute = true,
                    WorkingDirectory = BaseDirectory
                }
            };
            vrcxProcess.Start();
            Environment.Exit(0);
        }

        private static void OpenLink(string url)
        {
            if (url.StartsWith("http://") ||
                url.StartsWith("https://"))
            {
                Process.Start(new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                });
            }
        }

        #endregion
    }
}