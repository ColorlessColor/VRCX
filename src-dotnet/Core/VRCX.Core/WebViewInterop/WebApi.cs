using WebApiService = VRCX.Core.Services.WebApi.WebApiService;

namespace VRCX.Core.WebViewInterop;

public sealed class WebApi(WebApiService webApiService)
{
    public void ClearCookies() => webApiService.ClearCookies();

    public string GetCookies() => webApiService.GetCookies();
    public void SetCookies(string cookies) => webApiService.SetCookies(cookies);
    public Task<string> ExecuteJson(string options) => webApiService.ExecuteJson(options);
}