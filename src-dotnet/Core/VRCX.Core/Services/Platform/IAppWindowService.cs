namespace VRCX.Core.Services.Platform;

public interface IAppWindowService
{
    ValueTask FocusMainWindowAsync();
    ValueTask ChangeAppThemeAsync(AppTheme appTheme);
}

public enum AppTheme
{
    Dark = 1,
    Light = 0
}