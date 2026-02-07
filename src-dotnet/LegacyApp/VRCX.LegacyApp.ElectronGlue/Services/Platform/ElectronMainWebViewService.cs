using VRCX.Core.Services.Platform;

namespace VRCX.LegacyApp.ElectronGlue.Services.Platform;

public sealed class ElectronMainWebViewService : IMainWebViewService
{
    public Task ExecuteScriptAsync(string script)
    {
        throw new NotImplementedException();
    }

    public void ShowDevTools()
    {
        throw new NotImplementedException();
    }

    public ValueTask<double> GetZoomLevelAsync()
    {
        throw new NotImplementedException();
    }

    public Task SetZoomLevelAsync(double zoomLevel)
    {
        throw new NotImplementedException();
    }

    public ValueTask SetUserAgentAsync(string userAgent)
    {
        throw new NotImplementedException();
    }
}