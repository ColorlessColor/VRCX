using VRCX.Core.Services;
using WebApiService = VRCX.Core.Services.WebApi.WebApiService;

namespace VRCX.Core.WebViewInterop;

public sealed class WebApi(WebApiService webApiService)
{
    public void ClearCookies() => webApiService.ClearCookies();

    public string GetCookies() => webApiService.GetCookies();
    public void SetCookies(string cookies) => webApiService.SetCookies(cookies);
    public Task<string> ExecuteJson(string options) => webApiService.ExecuteJson(options);

    [Obsolete("Use ExecuteJson instead")]
    public Task<Tuple<int, string>> Execute(IDictionary<string, object> options)
    {
        throw new NotSupportedException("Use ExecuteJson instead");
    }

    [Obsolete("Do not use, cookies are automatically saved and loaded by WebApiService")]
    public void SaveCookies()
    {
        throw new InvalidOperationException("Do not use, cookies are automatically saved");
    }
}