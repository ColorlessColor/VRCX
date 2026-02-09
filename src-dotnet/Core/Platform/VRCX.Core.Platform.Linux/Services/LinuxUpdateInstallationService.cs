using System.Diagnostics;
using NLog;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Platform.Linux.Services;

public sealed class LinuxUpdateInstallationService(
    IPlatformLifetimeService platformLifetimeService
) : IUpdateInstallationService
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private static string InstallerPath => Path.Join(AppPathService.AppDataDirectory, "VRCX_Setup");

    public ValueTask<bool> IsInstallerReadyAsync()
    {
        return ValueTask.FromResult(File.Exists(InstallerPath));
    }

    public ValueTask PrepareUpdateInstallationAsync(string pathToInstaller)
    {
        if (File.Exists(InstallerPath))
        {
            _logger.Info("Deleting existing installer at {InstallerPath}", InstallerPath);
            File.Delete(InstallerPath);
        }

        File.Move(pathToInstaller, InstallerPath);
        return ValueTask.CompletedTask;
    }

    public async ValueTask InstallUpdateAsync()
    {
        if (!File.Exists(InstallerPath))
        {
            throw new FileNotFoundException("Installer not found", InstallerPath);
        }

        var vrcxProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = InstallerPath,
                UseShellExecute = true,
                WorkingDirectory = AppPathService.AppDataDirectory
            }
        };

        vrcxProcess.Start();

        await platformLifetimeService.InvokeShutdownAsync();
    }

    public ValueTask CleanupAfterInstallationAsync()
    {
        if (File.Exists(InstallerPath))
        {
            _logger.Info("Cleaning up installer at {InstallerPath}", InstallerPath);
            File.Delete(InstallerPath);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask CancelUpdateInstallationAsync()
    {
        if (File.Exists(InstallerPath))
        {
            _logger.Info("Cancelling update installation and deleting installer at {InstallerPath}", InstallerPath);
            File.Delete(InstallerPath);
        }

        return ValueTask.CompletedTask;
    }
}