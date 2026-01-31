using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using VRCX.App.Services;

namespace VRCX.App;

public partial class App
{
    public TrayIconService? TrayIconService { get; private set; }

    private void InitTrayIconService()
    {
        if (ServiceProvider is null)
            throw new InvalidOperationException("ServiceProvider is not initialized.");

        TrayIconService = ServiceProvider.GetRequiredService<TrayIconService>();
    }

    private void OnQuitClicked(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.InvokeShutdown();
    }

    private void OnOpenDevToolsClicked(object? sender, EventArgs e)
    {
        if (ServiceProvider is null)
            throw new InvalidOperationException("ServiceProvider is not initialized.");

        var webViewService = ServiceProvider.GetRequiredService<MainWebViewService>();
        webViewService.ShowDevTools();
    }

    private async void OnShowMainWindowClicked(object? sender, EventArgs e)
    {
        if (ServiceProvider is null)
            throw new InvalidOperationException("ServiceProvider is not initialized.");

        var appWindowService = ServiceProvider.GetRequiredService<AppWindowService>();
        await appWindowService.FocusMainWindowAsync();
    }
}