using System.Collections.Specialized;
using VRCX.Core.Services.Platform;
using VRCX.LegacyApp.WinFormsCef.CoreGlue.Utils;

namespace VRCX.LegacyApp.WinFormsCef.CoreGlue.Services.Platform;

public sealed class WinFormsClipboardService : IClipboardService
{
    public async ValueTask<string> GetClipboardAsString()
    {
        var tcs = new TaskCompletionSource<string>();

        var thread = new Thread(() => tcs.TrySetResult(Clipboard.GetText()));
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        return await tcs.Task;
    }

    public async Task SetBitmapAsync(string pathToImage)
    {
        await StaThreadUtils.RunOnStaThread(() =>
        {
            var image = Image.FromFile(pathToImage);
            var data = new DataObject();
            data.SetData(DataFormats.Bitmap, image);
            data.SetFileDropList(new StringCollection { pathToImage });
            Clipboard.SetDataObject(data, true);
        });
    }
}