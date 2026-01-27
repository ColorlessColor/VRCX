using System.Threading.Tasks;

namespace VRCX.Core.Services.Platform;

public interface IClipboardService
{
    ValueTask<string> GetClipboardAsString();
    Task SetBitmapAsync(string pathToImage);
}