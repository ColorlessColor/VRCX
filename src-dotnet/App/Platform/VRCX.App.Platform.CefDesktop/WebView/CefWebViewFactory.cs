using System;
using System.IO;
using System.Threading.Tasks;
using HarmonyLib;
using VRCX.App.WebView;
using Xilium.CefGlue;
using Xilium.CefGlue.Common;
using Xilium.CefGlue.Common.Handlers;
using Xilium.CefGlue.Common.Shared;

namespace VRCX.App.Platform.CefDesktop.WebView;

public sealed class CefWebViewFactory : IPlatformWebViewControlFactory
{
    public ValueTask InitializeAsync()
    {
        var harmony = new Harmony("VRCXArchValidation.CefDesktop.WebView.CefWebViewFactory");
        var method = AccessTools.Method("Xilium.CefGlue.Common.ObjectBinding.NativeObject:ToJavascriptMemberName");
        harmony.Patch(method,
            new HarmonyMethod(AccessTools.Method(typeof(CefWebViewFactory), nameof(ToJavascriptMemberName_Prefix))));

        CefRuntimeLoader.Initialize(new CefSettings(), customSchemes:
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

    private static bool ToJavascriptMemberName_Prefix(ref string __result, string name)
    {
        // Prevent conversion of method names to camelCase
        __result = name;
        return false; // Skip original method
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