using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using VRCX.App.Extensions;
using VRCX.App.Platform.Windows.Extensions;
using VRCX.Core.Platform.Windows.Extensions;

namespace VRCX.App.Platform.Windows;

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
        services.AddWindowsPlatformServices();
        services.AddWindowsWebViewServices();

        var app = services.BuildServiceProvider();
        app.RunApp(BuildAvaloniaApp, args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}