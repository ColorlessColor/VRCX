namespace VRCX.Core.Services.Platform;

public interface INativeMessageBoxService
{
    Task ShowAsync(string message, string title, NativeMessageBoxIcon icon);
}

public enum NativeMessageBoxIcon
{
    Error
}