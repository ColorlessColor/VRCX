using VRCX.Core.Services;

namespace VRCX.Core.WebViewInterop;

public class LogWatcher(LogWatcherService logWatcherService)
{
    // TODO: may got into trouble in json ipc
    public bool VrcClosedGracefully => logWatcherService.VrcClosedGracefully;

    public void Reset() => logWatcherService.Reset();
    public void SetDateTill(string date) => logWatcherService.SetDateTill(date);
    public List<string> GetLogLines() => logWatcherService.GetLogLines();
    public string[][] Get() => logWatcherService.Get();
}