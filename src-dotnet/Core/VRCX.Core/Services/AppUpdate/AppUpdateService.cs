using NLog;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Services.AppUpdate;

public sealed partial class AppUpdateService(
    IUpdateInstallationService updateInstallationService,
    WebApiService webApiService
)
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private string UpdateStatusFilePath => Path.Join(AppPathService.AppDataDirectory, "update_status.json");

    public async ValueTask InstallUpdateAsync()
    {
        _logger.Info("Starting update installation...");

        try
        {
            await updateInstallationService.InstallUpdateAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to install update.");
            throw;
        }
    }

    public async ValueTask CompleteInProgressUpdateIfSuccessAsync()
    {
        var status = await LoadUpdateStatus();
        if (status is null)
        {
            _logger.Info("No in-progress update found.");
            return;
        }

        if (AppBuildInfoService.Version != status.TargetVersion)
        {
            _logger.Info(
                "In-progress update target version {TargetVersion} does not match current version {CurrentVersion}",
                status.TargetVersion,
                AppBuildInfoService.Version);

            if (!await updateInstallationService.IsInstallerReadyAsync())
            {
                _logger.Warn("Installer for in-progress update is not ready. Cleaning up update status.");
                await updateInstallationService.CleanupAfterInstallationAsync();
                ClearUpdateStatus();
            }

            return;
        }

        _logger.Info("Completing in-progress update for version {TargetVersion}", status.TargetVersion);
        await updateInstallationService.CleanupAfterInstallationAsync();
        ClearUpdateStatus();
    }
}