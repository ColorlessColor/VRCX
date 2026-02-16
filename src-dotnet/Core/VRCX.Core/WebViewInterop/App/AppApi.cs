using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Serilog;
using VRCX.Core.Ipc;
using VRCX.Core.Models.Ipc;
using VRCX.Core.OverlayClient.XsOverlay.Services;
using VRCX.Core.Services;
using VRCX.Core.Services.AppUpdate;
using VRCX.Core.Services.Ipc;
using VRCX.Core.Services.Platform;
using VRCX.Core.Shared;

namespace VRCX.Core.WebViewInterop.App
{
    public partial class AppApi(
        AutoAppLaunchService appLaunchService,
        LogWatcherService logWatcherService,
        ImageCacheService imageCacheService,
        StartupArgsService startupArgsService,
        AppUpdateService appUpdateService,
        IPlatformLauncherService platformLauncherService,
        INotifyWebLoadedService notifyWebLoadedService,
        IpcServerService ipcServerService,
        XsOverlayClientService xsOverlayClientService)
    {
        private static readonly ILogger Logger = Log.ForContext<AppApi>();

        public void Init()
        {
        }

        public int GetColourFromUserID(string userId)
        {
            using var hasher = MD5.Create();
            var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(userId));
            return (hash[3] << 8) | hash[4];
        }

        public async Task OpenLink(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                Logger.Error("Blocked attempt to open link with invalid URL: {BlockedUrl}", url);
                return;
            }

            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                Logger.Error("Blocked attempt to open link with unsupported scheme: {BlockedUrl}", url);
                return;
            }

            await platformLauncherService.LaunchUriAsync(uri);
        }

        public string GetLaunchCommand()
        {
            return startupArgsService.LaunchArguments?.LaunchCommand ?? "";
        }

        public async Task IPCAnnounceStart()
        {
            await ipcServerService.SendAsync(new IpcOutPacketPayload("VRCXLaunch", null, "VRCXLaunch"));
        }

        public async Task SendIpc(string type, string data)
        {
            await ipcServerService.SendAsync(new IpcOutPacketPayload("VrcxMessage", data, type));
        }

        public string CustomCss()
        {
            var filePath = Path.Join(AppPathService.AppDataDirectory, "custom.css");
            if (File.Exists(filePath))
                return File.ReadAllText(filePath);

            return string.Empty;
        }

        public string CustomScript()
        {
            var filePath = Path.Join(AppPathService.AppDataDirectory, "custom.js");
            if (File.Exists(filePath))
                return File.ReadAllText(filePath);

            return string.Empty;
        }

        public string CurrentCulture()
        {
            var culture = CultureInfo.CurrentCulture.ToString();
            if (string.IsNullOrEmpty(culture))
                culture = "en-US";

            return culture;
        }

        public string CurrentLanguage()
        {
            return CultureInfo.InstalledUICulture.Name;
        }

        public string GetVersion()
        {
            return AppBuildInfoService.Version;
        }

        public bool VrcClosedGracefully()
        {
            return logWatcherService.VrcClosedGracefully;
        }

        public Dictionary<string, int> GetColourBulk(List<string> userIds)
        {
            var output = new Dictionary<string, int>();
            foreach (string userId in userIds)
            {
                output.Add(userId, GetColourFromUserID(userId));
            }

            return output;
        }

        public void SetAppLauncherSettings(bool enabled, bool killOnExit, bool runProcessOnce)
        {
            appLaunchService.Enabled = enabled;
            appLaunchService.KillChildrenOnExit = killOnExit;
            appLaunchService.RunProcessOnce = runProcessOnce;
        }

        public string? GetFileBase64(string path)
        {
            if (File.Exists(path))
            {
                return Convert.ToBase64String(File.ReadAllBytes(path));
            }

            return null;
        }

        public async Task<bool> TryOpenInstanceInVrc(string launchUrl)
        {
            return await VRChatIpcClient.SendAsync(launchUrl);
        }

        public async Task NotifyWebLoadedAsync()
        {
            await notifyWebLoadedService.NotifyWebLoadedAsync();
        }
    }
}