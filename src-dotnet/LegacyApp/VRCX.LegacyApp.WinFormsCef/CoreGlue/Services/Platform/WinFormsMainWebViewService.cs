using CefSharp;
using VRCX.Core.Services.Platform;
using VRCX.LegacyApp.WinFormsCef.Cef;

namespace VRCX.LegacyApp.WinFormsCef.CoreGlue.Services.Platform;

public sealed class WinFormsMainWebViewService : IMainWebViewService
{
    public Task ExecuteScriptAsync(string script)
    {
        if (MainForm.Instance?.Browser == null || MainForm.Instance.Browser.IsLoading ||
            !MainForm.Instance.Browser.CanExecuteJavascriptInMainFrame)
            return Task.CompletedTask;

        MainForm.Instance.Browser.ExecuteScriptAsync(script);
        return Task.CompletedTask;
    }

    public Task ExecuteScriptAsync(string methodName, params object[] args)
    {
        if (MainForm.Instance?.Browser == null || MainForm.Instance.Browser.IsLoading ||
            !MainForm.Instance.Browser.CanExecuteJavascriptInMainFrame)
            return Task.CompletedTask;

        MainForm.Instance.Browser.ExecuteScriptAsync("window?.$pinia?.vrcx.dragEnterCef", args);
        return Task.CompletedTask;
    }

    public void ShowDevTools()
    {
        MainForm.Instance.Browser.ShowDevTools();
    }

    public async ValueTask<double> GetZoomLevelAsync()
    {
        return await MainForm.Instance.Browser.GetZoomLevelAsync();
    }

    public Task SetZoomLevelAsync(double zoomLevel)
    {
        MainForm.Instance.Browser.SetZoomLevel(zoomLevel);
        return Task.CompletedTask;
    }

    public async ValueTask SetUserAgentAsync(string userAgent)
    {
        using var client = MainForm.Instance.Browser.GetDevToolsClient();
        await client.Network.SetUserAgentOverrideAsync(Program.Version);
    }
}