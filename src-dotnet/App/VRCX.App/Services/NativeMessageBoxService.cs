using System.Threading.Tasks;
using Avalonia.Threading;
using VRCX.App.Views;
using VRCX.Core.Services.Platform;

namespace VRCX.App.Services;

public sealed class NativeMessageBoxService : INativeMessageBoxService
{
    public async Task ShowAsync(string message, string title, NativeMessageBoxIcon icon)
    {
        await Dispatcher.UIThread.InvokeAsync(() => MessageBoxWindow.CreateAndShowAsync(message, title));
    }
}