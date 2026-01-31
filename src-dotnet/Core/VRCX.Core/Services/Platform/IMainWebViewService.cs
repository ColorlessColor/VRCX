using System.Text.Json;

namespace VRCX.Core.Services.Platform;

public interface IMainWebViewService
{
    Task ExecuteScriptAsync(string script);

    Task ExecuteScriptAsync(string methodName, params object[] args)
    {
        var argsJson = JsonSerializer.Serialize(args);
        var wrappedScript = $"{methodName}(...{argsJson})";
        return ExecuteScriptAsync(wrappedScript);
    }

    void ShowDevTools();
    ValueTask<double> GetZoomLevelAsync();
    Task SetZoomLevelAsync(double zoomLevel);
}