namespace VRCX.App.Platform.CefDesktop.WebView;

public sealed class CefWebViewMessageHostObject(Action<string> messageHandler)
{
    // ReSharper disable once InconsistentNaming
    public void postMessage(string message)
    {
        messageHandler(message);
    }
}