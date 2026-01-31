using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using VRCX.App.Extensions;
using VRCX.App.Ipc;
using VRCX.App.Services;
using VRCX.App.WebView;

namespace VRCX.App.ViewModels;

public sealed class MainWindowViewModel(
    IPlatformWebViewControlFactory webViewControlFactory,
    AppWindowService appWindowService,
    MainWebViewService mainWebViewService,
    WebViewJsonIpcService webViewJsonIpcService) : INotifyPropertyChanged
{
    public PlatformWebViewControl? WebViewControl
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public void SetMainWindow(Window mainWindow)
    {
        appWindowService.SetMainWindow(mainWindow);
    }

    public async Task LoadAsync()
    {
        // Notice: Running WebView initialization outside of UI thread will cause issues.
        await webViewControlFactory.InitializeAsync();
        var webview = await webViewControlFactory.CreateWebViewControlAsync();
        // due to bad design, must mount webview to visual tree before initialization
        WebViewControl = webview;

        await webview.InitializeAsync();
        webview.RegisterAppJavascriptObjects(webViewJsonIpcService);
        webview.Navigate("http://localhost:9000");

        mainWebViewService.SetWebViewControl(webview);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}