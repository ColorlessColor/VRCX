using DirectN;
using DirectN.Extensions.Com;
using VRCX.App.WebView;
using VRCX.Core.Shared;
using WebView2;
using WebView2.Utilities;

namespace VRCX.App.Platform.Windows.WebView;

public class WindowsWebViewControlFactory : IPlatformWebViewControlFactory
{
    private ComObject<ICoreWebView2Environment15>? _webView2Environment;

    public async ValueTask InitializeAsync()
    {
        var profilePath = Path.Combine(AppPathService.AppDataDirectory, "webview-profile", "webview2");

        var tcs = new TaskCompletionSource<ICoreWebView2Environment>();

        WebView2.Functions.CreateCoreWebView2EnvironmentWithOptions(PWSTR.Null, PWSTR.From(profilePath), null!,
            new CoreWebView2CreateCoreWebView2EnvironmentCompletedHandler((
                result, env) =>
            {
                tcs.SetResult(env);
            }));

        _webView2Environment = new ComObject<ICoreWebView2Environment15>(await tcs.Task);
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