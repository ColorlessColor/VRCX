using CefSharp;
using Microsoft.Extensions.DependencyInjection;
using VRCX.Core.WebViewInterop;
using VRCX.Core.WebViewInterop.App;
using VRCX.LegacyApp.WinFormsCef.CoreGlue.LegacySingleton;
using VRCXStorage = VRCX.Core.WebViewInterop.VRCXStorage;
using WebApi = VRCX.Core.WebViewInterop.WebApi;

namespace VRCX.LegacyApp.WinFormsCef.Cef
{
    public static class JavascriptBindings
    {
        public static void ApplyAppJavascriptBindings(IJavascriptObjectRepository repository)
        {
            repository.NameConverter = null;

            var provider = ServiceProviderInstance.Instance;
            repository.Register("AppApi", provider.GetRequiredService<AppApi>());
            repository.Register("WebApi", provider.GetRequiredService<WebApi>());
            repository.Register("VRCXStorage", provider.GetRequiredService<VRCXStorage>());
            repository.Register("SQLite", provider.GetRequiredService<SQLite>());
            repository.Register("LogWatcher", provider.GetRequiredService<LogWatcher>());
            repository.Register("Discord", provider.GetRequiredService<Discord>());
            repository.Register("AssetBundleManager", provider.GetRequiredService<AssetBundleManager>());
        }
    }
}
