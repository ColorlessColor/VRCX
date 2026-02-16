using System.Diagnostics;
using VRCX.Core;
using VRCX.Core.Services;
using VRCX.Core.Services.Platform;
using VRCX.Core.Shared;

namespace VRCX.LegacyApp.WinFormsCef.CoreGlue.Services.Platform;

public sealed class WinFormsPlatformLifetimeService(StartupArgsService startupArgsService) : IPlatformLifetimeService
{
    public ValueTask InvokeShutdownAsync()
    {
        Environment.Exit(0);
        return ValueTask.CompletedTask;
    }

    public ValueTask InvokeRestartAsync()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = Environment.ProcessPath,
            UseShellExecute = false,
            WorkingDirectory = AppPathService.AppDataDirectory
        };

        if (startupArgsService.LaunchArguments?.IsDebug == true)
            startInfo.ArgumentList.Add(VrcxLaunchArguments.IsDebugPrefix);

        if (!string.IsNullOrWhiteSpace(startupArgsService.LaunchArguments?.ProxyUrl))
            startInfo.ArgumentList.Add(
                $"{VrcxLaunchArguments.ProxyUrlPrefix}={startupArgsService.LaunchArguments.ProxyUrl}");

        startInfo.ArgumentList.Add(VrcxLaunchArguments.ConfigDirectoryPrefix);
        startInfo.ArgumentList.Add(AppPathService.AppDataDirectory);

        Process.Start(startInfo);

        Environment.Exit(0);
        return ValueTask.CompletedTask;
    }
}