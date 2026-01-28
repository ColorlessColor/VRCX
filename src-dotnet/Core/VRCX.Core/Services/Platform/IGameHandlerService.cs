using System;
using System.Threading.Tasks;

namespace VRCX.Core.Services.Platform;

public interface IGameHandlerService
{
    event EventHandler<bool>? OnGameRunningChanged;

    ValueTask<bool> IsGameRunningAsync();
    ValueTask<int> QuitGameAsync();
    ValueTask<bool> LaunchGameAsync(string arguments);
    ValueTask<bool> LaunchGameFromPathAsync(string gamePath, string arguments);
}