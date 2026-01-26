using VRCX.Core.Services;

namespace VRCX.Core.Extensions;

public static class ProcessMonitorExtenstion
{
    public static bool IsSteamVrRunning(this ProcessMonitorService processMonitorService)
    {
        return processMonitorService.IsProcessRunning("vrserver");
    }
}