using System.Diagnostics;
using Serilog;
using VRCX.Core.Services.Platform;
using VRCX.Core.Shared;

namespace VRCX.Core.Platform.Linux.Services;

public sealed class LinuxUpdateInstallationService(
    IPlatformLifetimeService platformLifetimeService
) : IUpdateInstallationService
{
    private readonly ILogger _logger = Log.ForContext<LinuxUpdateInstallationService>();

    private static string AppImagePathOld => Path.Join(AppPathService.AppDataDirectory, "VRCX.AppImage.old");

    private static string AppImagePathNew => Path.Join(AppPathService.AppDataDirectory, "VRCX.AppImage.new");

    public ValueTask<bool> IsInstallerReadyAsync()
    {
        return ValueTask.FromResult(File.Exists(AppImagePathNew));
    }

    public ValueTask PrepareUpdateInstallationAsync(string pathToInstaller)
    {
        if (File.Exists(AppImagePathNew))
        {
            _logger.Information("Deleting existing installer at {AppImagePathNew}", AppImagePathNew);
            File.Delete(AppImagePathNew);
        }

        File.Move(pathToInstaller, AppImagePathNew);
        return ValueTask.CompletedTask;
    }

    public async ValueTask InstallUpdateAsync()
    {
        // TODO implement linux installer
        if (!File.Exists(AppImagePathNew))
        {
            throw new FileNotFoundException("Installer not found", AppImagePathNew);
        }

        var runningAppImagePath = Process.GetCurrentProcess().MainModule?.FileName;
        if (runningAppImagePath == null || !File.Exists(runningAppImagePath))
        {
            throw new FileNotFoundException("Current VRCX executable file path not found", runningAppImagePath);
        }

        if (File.Exists(AppImagePathOld))
            File.Delete(AppImagePathOld);
        File.Move(runningAppImagePath, AppImagePathOld);
        File.Move(AppImagePathNew, runningAppImagePath);
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"+x {runningAppImagePath}"
            }
        };
        process.Start();
        await process.WaitForExitAsync();

        await platformLifetimeService.InvokeShutdownAsync();
    }

    public ValueTask CleanupAfterInstallationAsync()
    {
        if (File.Exists(AppImagePathNew))
        {
            _logger.Information("Cleaning up installer at {AppImagePathNew}", AppImagePathNew);
            File.Delete(AppImagePathNew);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask CancelUpdateInstallationAsync()
    {
        if (File.Exists(AppImagePathNew))
        {
            _logger.Information("Cancelling update installation and deleting installer at {AppImagePathNew}",
                AppImagePathNew);
            File.Delete(AppImagePathNew);
        }

        return ValueTask.CompletedTask;
    }
}