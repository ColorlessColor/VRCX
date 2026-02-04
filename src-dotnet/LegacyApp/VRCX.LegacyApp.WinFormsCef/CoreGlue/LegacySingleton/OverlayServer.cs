using VRCX.Core.Services;

namespace VRCX.LegacyApp.WinFormsCef.CoreGlue.LegacySingleton;

public static class OverlayServer
{
    public static OverlayWebSocketService Instance { get; set; }
}