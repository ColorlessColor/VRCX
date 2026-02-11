using System;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using VRCX.App.Extensions;
using VRCX.App.Platform.CefDesktop.Extensions;

#if WINDOWS
using VRCX.Core.Platform.Windows.Extensions;
#else
using VRCX.Core.Platform.Linux.Extensions;
#endif

namespace VRCX.App.Platform.CefDesktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddAppServices();
        services.AddCefWebViewServices();

#if WINDOWS
        services.AddWindowsPlatformServices();
#else
        services.AddLinuxPlatformServices();
#endif
        // TODO: Add other platform services here

        var app = services.BuildServiceProvider();
        app.RunApp(BuildAvaloniaApp, args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}