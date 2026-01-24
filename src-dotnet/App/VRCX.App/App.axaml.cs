using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using VRCX.App.Extensions;
using VRCX.App.Ipc;
using VRCX.App.Shared.WebView;
using VRCX.App.Views;

namespace VRCX.App;

public partial class App : Application
{
    public override void Initialize()
    {
        Program.Init("snapshot", ["--debug"]);
        WebViewJsonIpcService.Instance.RegisterJsonIpcApiObjects();
        PlatformWebViewControlFactory.Instance.InitializeAsync().GetAwaiter().GetResult();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}