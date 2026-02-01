namespace VRCX.Core.WebViewInterop.App;

public partial class AppApi
{
    public string GetOperatingSystem()
    {
        if (OperatingSystem.IsWindows())
            return "windows";

        if (OperatingSystem.IsLinux())
            return "linux";

        return "unknown";
    }

    public async Task DownloadUpdate(string targetVersion, string fileUrl, string hashString, int downloadSize)
    {
        await appUpdateService.DownloadAndPrepareUpdateAsync(targetVersion, fileUrl, hashString, downloadSize);
    }

    public async Task InstallUpdate()
    {
        if (await appUpdateService.GetInProgressUpdateTargetVersionAsync() == null)
        {
            logger.Warn("InstallUpdate called but no update is prepared");
            return;
        }

        await appUpdateService.InstallUpdateAsync();
    }

    public async Task CancelUpdate()
    {
        if (!appUpdateService.IsUpdateDownloading)
            return;

        await appUpdateService.CancelUpdateDownloadAsync();
    }

    public double CheckUpdateProgress()
    {
        return appUpdateService.DownloadProgress;
    }
}