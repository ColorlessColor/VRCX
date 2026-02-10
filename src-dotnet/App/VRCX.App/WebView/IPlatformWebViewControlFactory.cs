namespace VRCX.App.WebView;

public interface IPlatformWebViewControlFactory : IDisposable
{
    ValueTask InitializeAsync();
    ValueTask<PlatformWebViewControl> CreateWebViewControlAsync();
}