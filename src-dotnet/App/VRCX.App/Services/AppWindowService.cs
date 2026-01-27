using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using VRCX.Core.Services.Platform;

namespace VRCX.App.Services;

public class AppWindowService(ClassicDesktopStyleApplicationLifetime lifetime) : IAppWindowService
{
    private Window? _mainWindow;

    internal void SetMainWindow(Window window)
    {
        _mainWindow = window;
    }

    public TopLevel? GetTopLevel()
    {
        return _mainWindow ?? lifetime.Windows.FirstOrDefault();
    }
}