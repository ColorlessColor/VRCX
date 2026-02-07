using VRCX.Core.Services.Platform;

namespace VRCX.LegacyApp.ElectronGlue.Services.Platform;

public sealed class ElectronTrayIconService : ITrayIconService
{
    public ValueTask SetTrayIconNotificationAsync(bool notify)
    {
        throw new NotImplementedException();
    }
}