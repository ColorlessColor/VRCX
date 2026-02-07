using VRCX.Core.Services.Platform;

namespace VRCX.LegacyApp.ElectronGlue.Services.Platform;

public sealed class ElectronNativeMessageBoxService : INativeMessageBoxService
{
    public Task ShowAsync(string message, string title, NativeMessageBoxIcon icon)
    {
        throw new NotImplementedException();
    }
}