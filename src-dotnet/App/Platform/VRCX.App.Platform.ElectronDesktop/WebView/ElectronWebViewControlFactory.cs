using ElectronNET;
using VRCX.App.WebView;

namespace VRCX.App.Platform.ElectronDesktop.WebView;

public sealed class ElectronWebViewControlFactory : IPlatformWebViewControlFactory
{
    public async ValueTask InitializeAsync()
    {
        var runtimeController = ElectronNetRuntime.RuntimeController;

        await runtimeController.Start();
        await runtimeController.WaitReadyTask;
    }

    public ValueTask<PlatformWebViewControl> CreateWebViewControlAsync()
    {
        return ValueTask.FromResult<PlatformWebViewControl>(new ElectronWebViewControl());
    }
}