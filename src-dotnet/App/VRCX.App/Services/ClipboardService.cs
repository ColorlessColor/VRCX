using System.Threading.Tasks;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using VRCX.Core.Services.Platform;

namespace VRCX.App.Services;

public class ClipboardService(AppWindowService appWindowService) : IClipboardService
{
    public async ValueTask<string> GetClipboardAsString()
    {
        if (GetClipboard() is not { } clipboard)
            return "";

        return await clipboard.TryGetTextAsync() ?? "";
    }

    public async Task SetBitmapAsync(string pathToImage)
    {
        if (GetClipboard() is not { } clipboard)
            return;

        var bitmap = new Bitmap(pathToImage);
        // DO NOT dispose this bitmap
        // https://api-docs.avaloniaui.net/docs/M_Avalonia_Input_Platform_IClipboard_SetDataAsync#remarks
        await clipboard.SetBitmapAsync(bitmap);
    }

    private IClipboard? GetClipboard()
    {
        if (appWindowService.GetTopLevel() is not { } topLevel)
            return null;

        return topLevel.Clipboard;
    }
}