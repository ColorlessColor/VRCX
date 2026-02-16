using Microsoft.Web.WebView2.Core;
using VRCX.App.WebView;
using VRCX.Core;

namespace VRCX.App.Platform.Windows.WebView;

public class WindowsWebViewControlFactory : IPlatformWebViewControlFactory
{
    private CoreWebView2Environment? _webView2Environment;

    public async ValueTask InitializeAsync()
    {
        var profilePath = Path.Combine(AppPathService.AppDataDirectory, "webview-profile", "webview2");
        _webView2Environment = await CoreWebView2Environment.CreateAsync(userDataFolder: profilePath);
    }

    public ValueTask<PlatformWebViewControl> CreateWebViewControlAsync()
    {
        if (_webView2Environment is null)
            throw new InvalidOperationException("WebView2 environment is not initialized.");

        return ValueTask.FromResult<PlatformWebViewControl>(new WindowsWebViewControl(_webView2Environment));
    }

    public void Dispose()
    {
    }
}