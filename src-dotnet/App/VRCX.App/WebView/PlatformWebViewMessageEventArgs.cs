namespace VRCX.App.WebView;

public sealed class PlatformWebViewMessageEventArgs(string message) : EventArgs
{
    public string Message { get; } = message;
}