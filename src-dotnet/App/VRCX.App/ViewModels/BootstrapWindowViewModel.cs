using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Threading;
using NLog;
using VRCX.App.Extensions;
using VRCX.App.Services;
using VRCX.App.Views;
using VRCX.App.WebView;
using VRCX.App.WebViewInterop;
using VRCX.Core;
using VRCX.Core.Services.Platform;

namespace VRCX.App.ViewModels;

public sealed class BootstrapWindowViewModel(
    MainWindowViewModel mainWindowViewModel,
    NativeMessageBoxService nativeMessageBoxService,
    NotifyWebLoadedService notifyWebLoadedService,
    MainWebViewService mainWebViewService,
    WebViewJsonIpcService webViewJsonIpcService,
    BootstrapDelegate bootstrapDelegate
) : INotifyPropertyChanged
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public event EventHandler? RequestClose;

    public PlatformWebViewControl? WebViewControl
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string BootstrapMessage
    {
        get;
        private set
        {
            if (field == value)
                return;

            OnPropertyChanged();
            field = value;
        }
    } = "Starting...";

    public string Version
    {
        get
        {
            var versionString = AppBuildInfoService.Version;
            if (versionString.StartsWith("VRCX"))
            {
                versionString = versionString["VRCX ".Length..];
            }

            return versionString;
        }
    }

    public async Task BootstrapAsync()
    {
        try
        {
            await bootstrapDelegate();

            BootstrapMessage = "Initializing WebView...";

            // Notice: Running WebView initialization outside of UI thread will cause issues.
            WebViewControl = await mainWebViewService.GetOrCreateWebViewControlAsync();
            await WebViewControl.InitializeAsync();

            WebViewControl.RegisterAppJavascriptObjects(webViewJsonIpcService);
            WebViewControl.Navigate("http://localhost:9000");

            BootstrapMessage = "Waiting for Web App...";
            await notifyWebLoadedService.WaitForWebLoadedAsync();

            WebViewControl = null;

            var mainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };

            mainWindow.Show();
            mainWindow.Activate();
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "An error occurred during Bootstrap.");
            await nativeMessageBoxService.ShowAsync(
                ex.ToString(),
                "An error occurred during startup.",
                NativeMessageBoxIcon.Error);

            Dispatcher.UIThread.InvokeShutdown();
            return;
        }

        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed class BootstrapWindowViewModelFactory(
    MainWindowViewModel mainWindowViewModel,
    NativeMessageBoxService nativeMessageBoxService,
    NotifyWebLoadedService notifyWebLoadedService,
    MainWebViewService mainWebViewService,
    WebViewJsonIpcService webViewJsonIpcService
)
{
    public BootstrapWindowViewModel Create(BootstrapDelegate bootstrapDelegate) =>
        new(mainWindowViewModel,
            nativeMessageBoxService,
            notifyWebLoadedService,
            mainWebViewService,
            webViewJsonIpcService,
            bootstrapDelegate);
}

public delegate Task BootstrapDelegate();