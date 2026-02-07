using VRCX.Core.Services.Platform;

namespace VRCX.LegacyApp.ElectronGlue.Services.Platform;

public sealed class ElectronClipboardService : IClipboardService
{
    public ValueTask<string> GetClipboardAsString()
    {
        throw new NotImplementedException();
    }

    public Task SetBitmapAsync(string pathToImage)
    {
        throw new NotImplementedException();
    }
}