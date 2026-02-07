using VRCX.Core.Services.Platform;

namespace VRCX.LegacyApp.ElectronGlue.Services.Platform;

public sealed class ElectronLauncherService : IPlatformLauncherService
{
    public ValueTask<bool> LaunchUriAsync(Uri uri)
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> LaunchFileAsync(string filePath)
    {
        throw new NotImplementedException();
    }
}