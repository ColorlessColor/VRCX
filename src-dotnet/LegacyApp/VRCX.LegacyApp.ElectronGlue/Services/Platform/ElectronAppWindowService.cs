using VRCX.Core.Services.Platform;

namespace VRCX.LegacyApp.ElectronGlue.Services.Platform;

public sealed class ElectronAppWindowService : IAppWindowService
{
    public ValueTask FocusMainWindowAsync()
    {
        throw new NotImplementedException();
    }

    public ValueTask ChangeAppThemeAsync(AppTheme appTheme)
    {
        throw new NotImplementedException();
    }
}