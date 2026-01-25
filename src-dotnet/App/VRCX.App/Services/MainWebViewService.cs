using System.Threading.Tasks;
using VRCX.App.WebView;
using VRCX.Core.Services.Platform;

namespace VRCX.App.Services;

public sealed class MainWebViewService : IMainWebViewService
{
    private PlatformWebViewControl? _webViewControl;

    internal void SetWebViewControl(PlatformWebViewControl webViewControl)
    {
        _webViewControl = webViewControl;
    }

    public Task ExecuteScriptAsync(string methodName)
    {
        _webViewControl?.ExecuteScript(methodName);
        return Task.CompletedTask;
    }
}