namespace VRCX.Core.Services.Platform;

public interface IUpdateInstallationService
{
    ValueTask<bool> IsInstallerReadyAsync();

    ValueTask PrepareUpdateInstallationAsync(string pathToInstaller);
    ValueTask InstallUpdateAsync();
    ValueTask CleanupAfterInstallationAsync();
    ValueTask CancelUpdateInstallationAsync();
}