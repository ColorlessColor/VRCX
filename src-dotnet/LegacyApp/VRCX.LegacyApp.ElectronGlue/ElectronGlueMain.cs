using Microsoft.Extensions.DependencyInjection;
using Microsoft.JavaScript.NodeApi;
using VRCX.Core.Extensions;
using VRCX.Core.Services;
using VRCX.LegacyApp.ElectronGlue.Extensions;

#if WINDOWS
using VRCX.Core.Platform.Windows.Extensions;
#endif

namespace VRCX.LegacyApp.ElectronGlue;

[JSExport]
public class ElectronGlueMain
{
    private readonly ServiceProvider _serviceProvider;

    private bool _isInitialized;
    private bool _isShutdown;

    public ElectronGlueMain()
    {
        var builder = new ServiceCollection();

        builder.AddCoreServices();
        builder.AddElectronGlueServices();
#if WINDOWS
        builder.AddWindowsPlatformServices();
#endif

        _serviceProvider = builder.BuildServiceProvider();
        _serviceProvider.InitializeGlueClass();
    }

    public async Task InitializeAsync(string[] args)
    {
        if (_isShutdown)
            throw new InvalidOperationException("Already shutdown.");

        if (_isInitialized)
            throw new InvalidOperationException("Already initialized.");

        CoreLifetimeService.InitBeforeDiContainer(args);

        _isInitialized = true;
        var lifetimeService = _serviceProvider.GetRequiredService<CoreLifetimeService>();

        lifetimeService.PreInit(args);
        await lifetimeService.StartAsync(args);
    }

    public async Task ShutdownAsync()
    {
        if (_isInitialized)
            throw new InvalidOperationException("Not initialized.");

        if (_isShutdown)
            return;

        _isShutdown = true;
        var lifetimeService = _serviceProvider.GetRequiredService<CoreLifetimeService>();

        await lifetimeService.StopAsync();
    }
}