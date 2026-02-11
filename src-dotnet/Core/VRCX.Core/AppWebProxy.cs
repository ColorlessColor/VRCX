using System.Net;
using Serilog;
using VRCX.Core.Services;

namespace VRCX.Core;

public class AppWebProxy(
    AppStorageService appStorageService,
    StartupArgsService startupArgsService
) : IWebProxy
{
    private readonly ILogger _logger = Log.ForContext<AppWebProxy>();

    private CustomWebProxyInstance? _customWebProxyInstance;
    private Uri? _proxyUriOverride;

    public void Init()
    {
        if (startupArgsService.LaunchArguments?.ProxyUrl is not { } proxyUrl)
            return;

        if (string.IsNullOrWhiteSpace(proxyUrl))
        {
            _logger.Warning("Proxy URL in launch arguments is empty, will be ignore");
            return;
        }

        if (!Uri.TryCreate(proxyUrl, UriKind.Absolute, out var proxyUri) ||
            (!proxyUri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) &&
             !proxyUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)))
        {
            _logger.Warning("Proxy URL {ProxyUrl} in launch arguments is invalid, will be ignore", proxyUrl);
            return;
        }

        _proxyUriOverride = proxyUri;
    }

    public Uri? GetProxy(Uri destination)
    {
        return GetProxyCore()?.GetProxy(destination);
    }

    public bool IsBypassed(Uri host)
    {
        return GetProxyCore()?.IsBypassed(host) ?? true;
    }

    private IWebProxy? GetProxyCore()
    {
        var proxyUri = GetProxyUri();
        if (proxyUri is null)
        {
            return WebRequest.GetSystemWebProxy();
        }

        return GetCustomWebProxy(proxyUri);
    }

    private IWebProxy GetCustomWebProxy(Uri customProxyUri)
    {
        if (_customWebProxyInstance?.CustomProxyUri == customProxyUri)
            return _customWebProxyInstance.CustomProxy;

        var webProxy = new WebProxy(customProxyUri);
        _customWebProxyInstance = new CustomWebProxyInstance(webProxy, customProxyUri);

        return webProxy;
    }

    public Uri? GetProxyUri()
    {
        if (_proxyUriOverride is not null)
            return _proxyUriOverride;

        var proxyUri = appStorageService.Get("VRCX_ProxyServer");
        if (string.IsNullOrWhiteSpace(proxyUri))
        {
            return null;
        }

        if (!Uri.TryCreate(proxyUri, UriKind.Absolute, out var customProxyUri) ||
            (!customProxyUri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) &&
             !customProxyUri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)))
        {
            _logger.Warning("Proxy URI {ProxyUri} in storage is invalid, clearing it from storage", proxyUri);
            ClearProxyUriInStorage();
            return null;
        }

        return customProxyUri;
    }

    private void ClearProxyUriInStorage()
    {
        appStorageService.Remove("VRCX_ProxyServer");
    }

    private record CustomWebProxyInstance(IWebProxy CustomProxy, Uri CustomProxyUri);

    public ICredentials? Credentials { get; set; }
}