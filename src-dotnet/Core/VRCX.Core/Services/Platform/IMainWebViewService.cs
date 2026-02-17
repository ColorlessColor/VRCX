using System.Text.Json.Nodes;
using VRCX.Core.Utils;

namespace VRCX.Core.Services.Platform;

public interface IMainWebViewService
{
    Task ExecuteScriptAsync(string script);

    Task ExecuteScriptAsync(string methodName, params object[] args)
    {
        var argsInJsonValues = args.Select(JsonUtils.GetJsonValueFromBaseType).ToArray();
        var argsJson = new JsonArray(argsInJsonValues).ToJsonString();

        var wrappedScript = $"{methodName}(...{argsJson})";
        return ExecuteScriptAsync(wrappedScript);
    }

    void ShowDevTools();
    ValueTask<double> GetZoomLevelAsync();
    Task SetZoomLevelAsync(double zoomLevel);

    ValueTask SetUserAgentAsync(string userAgent);
}