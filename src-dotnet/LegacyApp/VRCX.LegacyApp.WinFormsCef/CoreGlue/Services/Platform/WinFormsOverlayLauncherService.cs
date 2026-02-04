using VRCX.Core.Services;
using VRCX.Core.Services.Platform;
using VRCX.LegacyApp.WinFormsCef.OverlayWebSocket;

namespace VRCX.LegacyApp.WinFormsCef.CoreGlue.Services.Platform;

public sealed class WinFormsOverlayLauncherService(StartupArgsService startupArgsService) : IOverlayLauncherService
{
    public void StartOverlay()
    {
        OverlayManager.StartOverlay();
    }
}