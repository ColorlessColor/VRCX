using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using VRCX.App.Extensions;
using VRCX.App.Shared.WebView;
using VRCX.App.Shared;

namespace VRCX.App.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                var webview = await PlatformWebViewControlFactory.Instance.CreateWebViewControlAsync();
                Content = webview;
                await webview.InitializeAsync();
                // webview.RegisterJavascriptObject("AppApi", new AppApi());
                webview.RegisterAppJavascriptObjects();

                webview.Navigate("http://localhost:9000/");
                // webview.Navigate("https://vrcx/index.html");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        });
    }
}