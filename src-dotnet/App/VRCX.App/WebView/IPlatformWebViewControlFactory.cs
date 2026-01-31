namespace VRCX.App.WebView;

public interface IPlatformWebViewControlFactory
{
    ValueTask InitializeAsync();
    ValueTask<PlatformWebViewControl> CreateWebViewControlAsync();
}