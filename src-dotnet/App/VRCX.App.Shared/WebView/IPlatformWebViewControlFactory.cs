namespace VRCX.App.Shared.WebView;

public interface IPlatformWebViewControlFactory
{
    ValueTask InitializeAsync();
    ValueTask<PlatformWebViewControl> CreateWebViewControlAsync();
}