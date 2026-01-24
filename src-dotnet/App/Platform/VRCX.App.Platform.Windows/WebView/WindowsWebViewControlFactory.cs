using System;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using VRCX.App.Shared.WebView;

namespace VRCX.App.Platform.Windows.WebView;

public class WindowsWebViewControlFactory : IPlatformWebViewControlFactory
{
    private CoreWebView2Environment? _webView2Environment;

    public async ValueTask InitializeAsync()
    {
        _webView2Environment = await CoreWebView2Environment.CreateAsync();
    }

    public ValueTask<PlatformWebViewControl> CreateWebViewControlAsync()
    {
        if (_webView2Environment is null)
            throw new InvalidOperationException("WebView2 environment is not initialized.");

        return ValueTask.FromResult<PlatformWebViewControl>(new WindowsWebViewControl(_webView2Environment));
    }
}