namespace VRCX.Core.Services.Platform;

public interface ITrayIconService
{
    ValueTask SetTrayIconNotificationAsync(bool notify);
}