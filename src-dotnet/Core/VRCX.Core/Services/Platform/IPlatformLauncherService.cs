namespace VRCX.Core.Services.Platform;

public interface IPlatformLauncherService
{
    ValueTask<bool> LaunchUriAsync(Uri uri);
    ValueTask<bool> LaunchFileAsync(string filePath);
}