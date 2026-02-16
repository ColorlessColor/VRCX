using System.Net;
using System.Text.Json;
using VRCX.Core.Models.WebApi;

namespace VRCX.Core.Services.WebApi;

public sealed partial class WebApiService
{
    private Task OnAfterHttpResponse(HttpResponseMessage response)
    {
        if (!response.Headers.Contains("Set-Cookie"))
            return Task.CompletedTask;

        _ = Task.Run(SaveCookies);
        return Task.CompletedTask;
    }

    public string GetCookies()
    {
        var cookiesJsonBytes = JsonSerializer.SerializeToUtf8Bytes(
            GetAllCookies(), WebApiJsonContext.Default.ListCookie
        );

        return Convert.ToBase64String(cookiesJsonBytes);
    }

    private List<Cookie> GetAllCookies()
    {
        return _cookieContainer.GetAllCookies().ToList();
    }

    private void LoadCookies()
    {
        _sqliteService.ExecuteNonQuery(
            "CREATE TABLE IF NOT EXISTS `cookies` (`key` TEXT PRIMARY KEY, `value` TEXT)");
        var values = _sqliteService.Execute("SELECT `value` FROM `cookies` WHERE `key` = @key",
            new Dictionary<string, object>
            {
                { "@key", "default" }
            }
        );
        try
        {
            var item = values[0];
            var cookies = DeserializeCookiesFromBase64((string)item[0]);

            _cookieContainer.Add(cookies);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to load cookies");
        }
    }

    public void SetCookies(string cookiesBase64)
    {
        _logger.Information("Setting cookies from web app");

        var cookies = DeserializeCookiesFromBase64(cookiesBase64);
        ClearCookies();
        _cookieContainer.Add(cookies);
    }

    public void SaveCookies()
    {
        try
        {
            _logger.Information("Saving cookies");
            var cookiesJsonBase64 = SerializeCookiesToBase64();

            _sqliteService.ExecuteNonQuery(
                "INSERT OR REPLACE INTO `cookies` (`key`, `value`) VALUES (@key, @value)",
                new Dictionary<string, object>()
                {
                    { "@key", "default" },
                    { "@value", cookiesJsonBase64 }
                }
            );
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to save cookies");
        }
    }

    public void ClearCookies()
    {
        _logger.Information("Clearing cookies");

        // TODO: Delete cookies for WebView
        foreach (Cookie cookie in _cookieContainer.GetAllCookies())
        {
            cookie.Expired = true;
        }

        SaveCookies();
    }

    #region Cookies Serialization

    private static CookieCollection DeserializeCookiesFromBase64(string base64Cookies)
    {
        var cookiesJsonBytes = Convert.FromBase64String(base64Cookies);
        if (JsonSerializer.Deserialize<CookieCollection>(
                cookiesJsonBytes, WebApiJsonContext.Default.CookieCollection
            ) is not { } cookieCollection)
            throw new ArgumentException("Cookies data are a null json", nameof(base64Cookies));

        return cookieCollection;
    }

    private string SerializeCookiesToBase64()
    {
        var cookiesJsonBytes = JsonSerializer.SerializeToUtf8Bytes(
            GetAllCookies(),
            WebApiJsonContext.Default.ListCookie
        );

        return Convert.ToBase64String(cookiesJsonBytes);
    }

    #endregion
}