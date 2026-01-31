using VRCX.Core.Extensions;

namespace VRCX.Core.AppApi;

public partial class AppApiCore
{
    private void RegisterGameHandlerEvents()
    {
        _gameHandlerService.OnGameRunningChanged += async (_, _) => await CheckGameRunning();
    }

    public override async Task CheckGameRunning()
    {
        var isGameRunning = await IsGameRunning();
        var isSteamVRRunning = _processMonitorService.IsProcessRunning("vrserver");

        await _mainWebViewService.ExecuteScriptAsync(
            "window?.$pinia?.game.updateIsGameRunning",
            isGameRunning,
            isSteamVRRunning
        );
    }

    public override async Task<bool> IsGameRunning() => await _gameHandlerService.IsGameRunningAsync();

    public override bool IsSteamVRRunning()
    {
        // unused
        return _processMonitorService.IsSteamVrRunning();
    }

    public override async Task<int> QuitGame() => await _gameHandlerService.QuitGameAsync();

    public override async Task<bool> StartGame(string arguments) =>
        await _gameHandlerService.LaunchGameAsync(arguments);

    public override async Task<bool> StartGameFromPath(string path, string arguments) =>
        await _gameHandlerService.LaunchGameFromPathAsync(path, arguments);
}