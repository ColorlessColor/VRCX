using System;
using Avalonia.Controls;
using Avalonia.Platform;

namespace VRCX.App.Utils;

public static class BoundsUtils
{
    public static Screen? GetScreenFromWindow(WindowBase? window)
    {
        if (window != null)
        {
            var result = window.Screens.ScreenFromWindow(window);
            return result;
        }

        return null;
    }

    public static int ToPxSize(double d, Screen? screen)
    {
        if (double.IsNaN(d) || d <= 0D)
        {
            return 0;
        }

        if (screen != null)
        {
            d *= screen.Scaling;
        }

        var result = Math.Ceiling(d);
        return Convert.ToInt32(result);
    }
}