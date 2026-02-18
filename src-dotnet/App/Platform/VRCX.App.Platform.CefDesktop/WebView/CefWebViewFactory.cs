using VRCX.App.WebView;
using VRCX.Core.Shared;
using Xilium.CefGlue;
using Xilium.CefGlue.Common;
using Xilium.CefGlue.Common.Handlers;
using Xilium.CefGlue.Common.Shared;

namespace VRCX.App.Platform.CefDesktop.WebView;

public sealed class CefWebViewFactory : IPlatformWebViewControlFactory
{
    public ValueTask InitializeAsync()
    {
        var profilePath = Path.Combine(AppPathService.AppDataDirectory, "webview-profile", "cefglue");

        CefRuntimeLoader.Initialize(new CefSettings
            {
                RootCachePath = profilePath,
                CachePath = profilePath
            }, customSchemes:
            [
                new CustomScheme
                {
                    SchemeName = "https",
                    DomainName = "vrcx",
                    SchemeHandlerFactory = new AssetSchemeHandlerFactory()
                }
            ]);

        return ValueTask.CompletedTask;
    }

    public ValueTask<PlatformWebViewControl> CreateWebViewControlAsync()
    {
        PlatformWebViewControl control = new CefWebViewControl();
        return ValueTask.FromResult(control);
    }

    public void Dispose()
    {
    }
}

public class AssetSchemeHandlerFactory : CefSchemeHandlerFactory
{
    protected override CefResourceHandler Create(CefBrowser browser, CefFrame frame, string schemeName,
        CefRequest request)
    {
        var uri = new Uri(request.Url);
        var assetsFilePath = uri.LocalPath;
        try
        {
            var fileStream = File.OpenRead(Path.Join(AppContext.BaseDirectory, "html", assetsFilePath));

            var handler = new DefaultResourceHandler
            {
                Response = fileStream,
            };

            return handler;
        }
        catch (Exception ex)
        {
            return new DefaultResourceHandler
            {
                Status = 404,
                StatusText = "Not Found",
            };
        }
    }
}