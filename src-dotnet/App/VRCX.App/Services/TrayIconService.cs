using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Threading;
using VRCX.Core.Services.Platform;

namespace VRCX.App.Services;

public sealed class TrayIconService : ITrayIconService, INotifyPropertyChanged
{
    public async ValueTask SetTrayIconNotificationAsync(bool notify)
    {
        await Dispatcher.UIThread.InvokeAsync(() => HasNotification = notify);
    }

    #region View Model

    public bool HasNotification
    {
        get;
        private set
        {
            if (value == field) return;
            field = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}