using VRCX.Core.Models.GamePlayerPrefs;
using VRCX.Core.Services;

namespace VRCX.Core.WebViewInterop.App
{
    public abstract partial class AppApi
    {
        // AppApi
        public abstract void ShowDevTools();
        public abstract void SetVR(bool active, bool hmdOverlay, bool wristOverlay, bool menuButton, int overlayHand);
        public abstract Task SetZoom(double zoomLevel);
        public abstract Task<double> GetZoom();
        public abstract Task DesktopNotification(string BoldText, string Text = "", string Image = "");
        public abstract Task SetTrayIconNotification(bool notify);

        public abstract Task RestartApplication(bool isUpgrade);
        public abstract Task<bool> CheckForUpdateExe();
        public abstract void ExecuteVrOverlayFunction(string function, string json);
        public abstract Task FocusWindow();
        public abstract Task ChangeTheme(int value);

        public abstract Task<string> GetClipboard();
        public abstract Task SetStartup(bool enabled);
        public abstract Task CopyImageToClipboard(string path);

        public abstract Task SetUserAgent();
        public abstract Task OpenCalendarFile(string icsContent);

        // Folders
        public abstract string GetVRChatAppDataLocation();
        public abstract string GetVRChatPhotosLocation();
        public abstract string GetUGCPhotoLocation(string path = "");
        public abstract string GetVRChatScreenshotsLocation();
        public abstract string GetVRChatCacheLocation();
        public abstract Task<bool> OpenVrcxAppDataFolder();
        public abstract Task<bool> OpenVrcAppDataFolder();
        public abstract Task<bool> OpenVrcPhotosFolder();
        public abstract Task<bool> OpenUGCPhotosFolder(string ugcPath = "");
        public abstract Task<bool> OpenVrcScreenshotsFolder();
        public abstract Task<bool> OpenCrashVrcCrashDumps();
        public abstract Task OpenShortcutFolder();
        public abstract Task OpenFolderAndSelectItem(string path, bool isFolder = false);
        public abstract Task<string> OpenFolderSelectorDialog(string defaultPath = "");

        public abstract Task<string> OpenFileSelectorDialog(string defaultPath = "", string defaultExt = "",
            string defaultFilter = "All files (*.*)|*.*");

        public abstract Task CheckGameRunning();
        public abstract bool IsGameRunning();
        public abstract bool IsSteamVRRunning();
        public abstract Task<int> QuitGame();
        public abstract Task<bool> StartGame(string arguments);
        public abstract Task<bool> StartGameFromPath(string path, string arguments);

        // RegistryPlayerPrefs
        public abstract Task<string> GetVRChatRegistryKeyAsJsonString(string key);

        public abstract Task<bool> SetVRChatRegistryKeyFromJsonString(
            string key, string valueAsJsonString, int typeInt
        );

        public abstract Task<Dictionary<string, RegistryKeyValue>> GetVRChatRegistry();
        public abstract Task SetVRChatRegistry(string json);
        public abstract Task<bool> HasVRChatRegistryFolder();
        public abstract Task DeleteVRChatRegistryFolder();
        public abstract string ReadVrcRegJsonFile(string filepath);

        // Screenshot
        public abstract string AddScreenshotMetadata(string path, string metadataString, string worldId,
            bool changeFilename = false);
    }
}