using VRCX.Core.Services;
using VRCX.Core.Services.Platform;
using VRCX.Core.Shared;
using VRCX.LegacyApp.WinFormsCef.OverlayWebSocket;

namespace VRCX.LegacyApp.WinFormsCef.CoreGlue.Services.Platform;

public sealed class WinFormsOverlayLauncherService : IOverlayLauncherService
{
    public void StartOverlay()
    {
        OverlayManager.StartOverlay();
    }
}