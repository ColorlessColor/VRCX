using System.Threading.Tasks;

namespace VRCX.Core.WebViewInterop.App;

public partial class AppApi
{
    public async Task DownloadUpdate(string fileUrl, string hashString, int downloadSize)
    {
        // TODO: Re-implement update downloading
        // await Update.DownloadUpdate(fileUrl, hashString, downloadSize);
        return;
    }

    public void CancelUpdate()
    {
        // TODO: Re-implement update cancelling
        // Update.CancelUpdate();
    }
    
    public int CheckUpdateProgress()
    {
        // TODO: Re-implement update progress checking
        //return Update.UpdateProgress;

        return 0;
    }
}