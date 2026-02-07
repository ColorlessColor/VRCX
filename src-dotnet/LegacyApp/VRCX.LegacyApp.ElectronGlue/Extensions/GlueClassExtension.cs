using Microsoft.Extensions.DependencyInjection;
using VRCX.Core.WebViewInterop;
using VRCX.Core.WebViewInterop.App;
using VRCX.LegacyApp.ElectronGlue.GlueClass;

namespace VRCX.LegacyApp.ElectronGlue.Extensions;

internal static class GlueClassExtension
{
    public static ServiceProvider InitializeGlueClass(this ServiceProvider provider)
    {
        ExportToWebView.VRCXStorage = provider.GetRequiredService<VRCXStorage>();
        ExportToWebView.AppApi = provider.GetRequiredService<AppApi>();
        ExportToWebView.AssetBundleManager = provider.GetRequiredService<AssetBundleManager>();
        ExportToWebView.LogWatcher = provider.GetRequiredService<LogWatcher>();
        ExportToWebView.SQLite = provider.GetRequiredService<SQLite>();
        ExportToWebView.WebApi = provider.GetRequiredService<WebApi>();

        return provider;
    }
}