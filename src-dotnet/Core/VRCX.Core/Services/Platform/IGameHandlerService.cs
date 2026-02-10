namespace VRCX.Core.Services.Platform;

public interface IGameHandlerService
{
    ValueTask<int> QuitGameAsync();
    ValueTask<bool> LaunchGameAsync(string arguments);
    ValueTask<bool> LaunchGameFromPathAsync(string gamePath, string arguments);
}