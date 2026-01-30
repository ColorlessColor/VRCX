using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
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
        public abstract void DesktopNotification(string BoldText, string Text = "", string Image = "");
        public abstract void SetTrayIconNotification(bool notify);

        public abstract void RestartApplication(bool isUpgrade);
        public abstract bool CheckForUpdateExe();
        public abstract void ExecuteVrOverlayFunction(string function, string json);
        public abstract void FocusWindow();
        public abstract void ChangeTheme(int value);
        public abstract void DoFunny();
        public abstract Task<string> GetClipboard();
        public abstract void SetStartup(bool enabled);
        public abstract Task CopyImageToClipboard(string path);

        [Obsolete("Use Desktop Notifications instead")]
        public virtual void FlashWindow()
        {
        }

        public abstract void SetUserAgent();
        public abstract void OpenCalendarFile(string icsContent);

        // Folders
        public abstract string GetVRChatAppDataLocation();
        public abstract string GetVRChatPhotosLocation();
        public abstract string GetUGCPhotoLocation(string path = "");
        public abstract string GetVRChatScreenshotsLocation();
        public abstract string GetVRChatCacheLocation();
        public abstract bool OpenVrcxAppDataFolder();
        public abstract bool OpenVrcAppDataFolder();
        public abstract bool OpenVrcPhotosFolder();
        public abstract bool OpenUGCPhotosFolder(string ugcPath = "");
        public abstract bool OpenVrcScreenshotsFolder();
        public abstract bool OpenCrashVrcCrashDumps();
        public abstract void OpenShortcutFolder();
        public abstract void OpenFolderAndSelectItem(string path, bool isFolder = false);
        public abstract Task<string> OpenFolderSelectorDialog(string defaultPath = "");

        public abstract Task<string> OpenFileSelectorDialog(string defaultPath = "", string defaultExt = "",
            string defaultFilter = "All files (*.*)|*.*");

        // GameHandler
        [Obsolete("Use IGameHandlerService.OnGameRunningChanged event instead")]
        public virtual void OnProcessStateChanged(MonitoredProcess monitoredProcess)
        {
        }

        public abstract Task CheckGameRunning();
        public abstract Task<bool> IsGameRunning();
        public abstract bool IsSteamVRRunning();
        public abstract Task<int> QuitGame();
        public abstract Task<bool> StartGame(string arguments);
        public abstract Task<bool> StartGameFromPath(string path, string arguments);

        // RegistryPlayerPrefs
        public abstract Task<object?> GetVRChatRegistryKey(string key);
        public abstract Task<string?> GetVRChatRegistryKeyString(string key);
        public abstract Task<bool> SetVRChatRegistryKey(string key, object value, int typeInt);

        [Obsolete("Use SetVRChatRegistryKey with appropriate typeInt instead")]
        public virtual void SetVRChatRegistryKey(string key, byte[] value)
        {
        }

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