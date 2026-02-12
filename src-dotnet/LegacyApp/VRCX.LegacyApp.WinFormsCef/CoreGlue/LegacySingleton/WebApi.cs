using VRCX.Core;
using VRCX.Core.Services;
using WebApiService = VRCX.Core.Services.WebApi.WebApiService;

namespace VRCX.LegacyApp.WinFormsCef.CoreGlue.LegacySingleton;

public static class WebApi
{
    public static WebApiService Instance { get; set; }
    public static AppWebProxy Proxy { get; set; }
}