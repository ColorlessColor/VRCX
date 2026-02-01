using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using NLog;
using VRCX.Core.Services;
using VRCX.Core.Services.AppUpdate;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.WebViewInterop.App
{
    public partial class AppApi(
        AutoAppLaunchService appLaunchService,
        LogWatcherService logWatcherService,
        ImageCacheService imageCacheService,
        StartupArgsService startupArgsService,
        AppUpdateService appUpdateService,
        IPlatformLauncherService platformLauncherService)
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public void Init()
        {
        }

        public JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            Error = delegate(object _, Newtonsoft.Json.Serialization.ErrorEventArgs args)
            {
                args.ErrorContext.Handled = true;
            }
        };

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
                logger.Error("Blocked attempt to open link with invalid URL: {BlockedUrl}", url);
                return;
            }

            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                logger.Error("Blocked attempt to open link with unsupported scheme: {BlockedUrl}", url);
                return;
            }

            await platformLauncherService.LaunchUriAsync(uri);
        }

        public string GetLaunchCommand()
        {
            return startupArgsService.LaunchArguments?.LaunchCommand ?? "";
        }

        public void IPCAnnounceStart()
        {
            IPCServer.Send(new IPCPacket
            {
                Type = "VRCXLaunch",
                MsgType = "VRCXLaunch"
            });
        }

        public void SendIpc(string type, string data)
        {
            IPCServer.Send(new IPCPacket
            {
                Type = "VrcxMessage",
                MsgType = type,
                Data = data
            });
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

        public Dictionary<string, int> GetColourBulk(List<object> userIds)
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

        public string GetFileBase64(string path)
        {
            if (File.Exists(path))
            {
                return Convert.ToBase64String(File.ReadAllBytes(path));
            }

            return null;
        }

        public Task<bool> TryOpenInstanceInVrc(string launchUrl)
        {
            return VRCIPC.Send(launchUrl);
        }
    }
}