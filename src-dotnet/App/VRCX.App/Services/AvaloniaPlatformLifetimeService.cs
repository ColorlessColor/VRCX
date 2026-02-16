using System.Diagnostics;
using Avalonia.Threading;
using VRCX.Core;
using VRCX.Core.Services;
using VRCX.Core.Services.Platform;
using VRCX.Core.Shared;

namespace VRCX.App.Services;

public sealed class AvaloniaPlatformLifetimeService(StartupArgsService startupArgsService) : IPlatformLifetimeService
{
    public ValueTask InvokeShutdownAsync()
    {
        Dispatcher.UIThread.InvokeShutdown();
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

        Dispatcher.UIThread.InvokeShutdown();
        return ValueTask.CompletedTask;
    }
}