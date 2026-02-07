namespace VRCX.Core.Services.Platform;

public interface IGameRunningStatusService
{
    event EventHandler<bool>? OnGameRunningChanged;

    bool IsGameRunning();
    bool IsSteamVRRunning();
}