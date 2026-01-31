using VRCX.Core.Services;

namespace VRCX.Core.WebViewInterop;

public sealed class WebApi(WebApiService webApiService)
{
    public void ClearCookies() => webApiService.ClearCookies();
    public void SaveCookies() => webApiService.SaveCookies();
    public string GetCookies() => webApiService.GetCookies();
    public void SetCookies(string cookies) => webApiService.SetCookies(cookies);
    public Task<string> ExecuteJson(string options) => webApiService.ExecuteJson(options);
    public Task<Tuple<int, string>> Execute(IDictionary<string, object> options) => webApiService.Execute(options);
}