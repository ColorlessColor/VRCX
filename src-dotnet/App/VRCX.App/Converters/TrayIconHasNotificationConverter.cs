using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace VRCX.App.Converters;

public class TrayIconHasNotificationConverter : IValueConverter
{
    private readonly WindowIcon _defaultIcon =
        new(LoadBitmapFromResource("avares://VRCX.App/Assets/VRCX.ico"));

    private readonly WindowIcon _notificationIcon =
        new(LoadBitmapFromResource("avares://VRCX.App/Assets/VRCX_notify.ico"));

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            true => _notificationIcon,
            _ => _defaultIcon
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }

    private static Bitmap LoadBitmapFromResource(string uri)
    {
        var assets = AssetLoader.Open(new Uri(uri));
        return new Bitmap(assets);
    }
}