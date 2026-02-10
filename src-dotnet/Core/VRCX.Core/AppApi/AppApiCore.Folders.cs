using VRCX.Core.Utils;

namespace VRCX.Core.AppApi;

public partial class AppApiCore
{
    public override string GetVRChatAppDataLocation()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"Low\VRChat\VRChat";
    }

    public override string GetVRChatCacheLocation() => _gameFolderProvider.GetVRChatCacheLocation();

    public override string GetVRChatPhotosLocation() => _gameFolderProvider.GetVRChatPhotosLocation();

    public override string GetUGCPhotoLocation(string path = "")
    {
        if (string.IsNullOrEmpty(path))
        {
            return GetVRChatPhotosLocation();
        }

        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to create UGC photo directory at {Path}", path);
            return GetVRChatPhotosLocation();
        }
    }

    private string GetSteamUserdataPathFromRegistry() => _gameFolderProvider.GetSteamUserdataPath();

    public override string GetVRChatScreenshotsLocation()
    {
        // program files steam userdata screenshots
        var steamUserdataPath = GetSteamUserdataPathFromRegistry();
        var screenshotPath = string.Empty;
        var latestWriteTime = DateTime.MinValue;
        if (!Directory.Exists(steamUserdataPath))
            return screenshotPath;

        var steamUserDirs = Directory.GetDirectories(steamUserdataPath);
        foreach (var steamUserDir in steamUserDirs)
        {
            var screenshotDir = Path.Join(steamUserDir, "760", "remote", VRChatUtils.VRChatSteamAppid, "screenshots");
            if (!Directory.Exists(screenshotDir))
                continue;

            var lastWriteTime = File.GetLastWriteTime(screenshotDir);
            if (lastWriteTime <= latestWriteTime)
                continue;

            latestWriteTime = lastWriteTime;
            screenshotPath = screenshotDir;
        }

        return screenshotPath;
    }

    public override async Task<bool> OpenVrcxAppDataFolder()
    {
        var path = AppPathService.AppDataDirectory;
        if (!Directory.Exists(path))
            return false;

        await OpenFolderAndSelectItem(path, true);
        return true;
    }

    public override async Task<bool> OpenVrcAppDataFolder()
    {
        var path = _gameFolderProvider.GetVRChatAppDataLocation();
        if (!Directory.Exists(path))
            return false;

        await OpenFolderAndSelectItem(path, true);
        return true;
    }

    public override async Task<bool> OpenVrcPhotosFolder()
    {
        var path = GetVRChatPhotosLocation();
        if (!Directory.Exists(path))
            return false;

        await OpenFolderAndSelectItem(path, true);
        return true;
    }

    public override async Task<bool> OpenUGCPhotosFolder(string ugcPath = "")
    {
        var path = GetUGCPhotoLocation(ugcPath);
        if (!Directory.Exists(path))
            return false;

        await OpenFolderAndSelectItem(path, true);
        return true;
    }

    public override async Task<bool> OpenVrcScreenshotsFolder()
    {
        var path = GetVRChatScreenshotsLocation();
        if (!Directory.Exists(path))
            return false;

        await OpenFolderAndSelectItem(path, true);
        return true;
    }

    public override async Task<bool> OpenCrashVrcCrashDumps()
    {
        var path = _gameFolderProvider.GetVRChatCrashDumpsLocation();
        if (!Directory.Exists(path))
            return false;

        await OpenFolderAndSelectItem(path, true);
        return true;
    }

    public override async Task OpenShortcutFolder()
    {
        var path = _appLaunchService.AppShortcutDirectory;
        if (!Directory.Exists(path))
            return;

        await OpenFolderAndSelectItem(path, true);
    }

    public override async Task OpenFolderAndSelectItem(string path, bool isFolder = false) =>
        await _fileDialogService.HighlightInFileExplorerAsync(path);

    public override async Task<string> OpenFolderSelectorDialog(string defaultPath = "")
    {
        var initialLocation = Directory.Exists(defaultPath) ? defaultPath : GetVRChatPhotosLocation();
        return await _fileDialogService.OpenFolderSelectorDialogAsync(initialLocation);
    }

    public override async Task<string> OpenFileSelectorDialog(string defaultPath = "", string defaultExt = "",
        string defaultFilter = "All files (*.*)|*.*")
    {
        return await _fileDialogService.OpenFileSelectorDialogAsync(defaultPath, defaultExt, defaultFilter);
    }
}