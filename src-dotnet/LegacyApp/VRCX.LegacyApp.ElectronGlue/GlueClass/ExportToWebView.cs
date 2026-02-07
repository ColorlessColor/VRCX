using VRCX.Core.WebViewInterop;
using VRCX.Core.WebViewInterop.App;

namespace VRCX.LegacyApp.ElectronGlue.GlueClass;

public static class ExportToWebView
{
    public static AppApi AppApi { get; set; }
    public static VRCXStorage VRCXStorage { get; set; }
    public static AssetBundleManager AssetBundleManager { get; set; }
    public static LogWatcher LogWatcher { get; set; }
    public static SQLite SQLite { get; set; }
    public static WebApi WebApi { get; set; }
}