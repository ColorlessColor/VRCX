namespace VRCX.Core.AppApi;

public partial class AppApiCore
{
    private void RegisterGameHandlerEvents()
    {
        _gameRunningStatusService.OnGameRunningChanged += async (_, _) => await CheckGameRunning();
    }

    public override async Task CheckGameRunning()
    {
        var isGameRunning = IsGameRunning();
        var isSteamVrRunning = IsSteamVRRunning();

        await _mainWebViewService.ExecuteScriptAsync(
            "window?.$pinia?.game.updateIsGameRunning",
            isGameRunning,
            isSteamVrRunning
        );
    }

    public override bool IsGameRunning() => _gameRunningStatusService.IsGameRunning();

    public override bool IsSteamVRRunning() => _gameRunningStatusService.IsSteamVRRunning();

    public override async Task<int> QuitGame() => await _gameHandlerService.QuitGameAsync();

    public override async Task<bool> StartGame(string arguments) =>
        await _gameHandlerService.LaunchGameAsync(arguments);

    public override async Task<bool> StartGameFromPath(string path, string arguments) =>
        await _gameHandlerService.LaunchGameFromPathAsync(path, arguments);
}