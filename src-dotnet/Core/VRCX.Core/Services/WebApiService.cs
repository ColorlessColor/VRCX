using System.Collections;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using SixLabors.ImageSharp;
using VRCX.Core.Services.Platform;
using VRCX.Core.Utils;

namespace VRCX.Core.Services;

public sealed class WebApiService : IDisposable
{
    private static readonly ILogger Logger = Log.ForContext<WebApiService>();

    public bool ProxySet;
    public string ProxyUrl = "";
    public IWebProxy Proxy = HttpClient.DefaultProxy;

    public CookieContainer CookieContainer = new();
    private bool _cookieDirty;
    private readonly Timer _timer;

    private HttpClient? _httpClient;
    private SocketsHttpHandler? _httpHandler;

    private readonly AppStorageService _appStorageService;
    private readonly SqliteService _sqliteService;
    private readonly StartupArgsService _startupArgsService;
    private readonly INativeMessageBoxService _messageBoxService;
    private readonly IPlatformLifetimeService _platformLifetimeService;

    public WebApiService(
        AppStorageService appStorageService,
        SqliteService sqliteService,
        StartupArgsService startupArgsService,
        INativeMessageBoxService messageBoxService,
        IPlatformLifetimeService platformLifetimeService)
    {
        _appStorageService = appStorageService;
        _sqliteService = sqliteService;
        _startupArgsService = startupArgsService;
        _messageBoxService = messageBoxService;
        _platformLifetimeService = platformLifetimeService;

        _timer = new Timer(TimerCallback, null, -1, -1);
    }

    private void TimerCallback(object? state)
    {
        try
        {
            SaveCookies();
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to save cookies");
        }
    }

    public void Init()
    {
        SetProxy();
        InitializeHttpClient();
        LoadCookies();
        _timer.Change(1000, 1000);
    }

    private void InitializeHttpClient()
    {
        _httpClient?.Dispose();

        _httpHandler = new SocketsHttpHandler
        {
            CookieContainer = CookieContainer,
            UseCookies = true,
            AutomaticDecompression = DecompressionMethods.All,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            MaxConnectionsPerServer = 10
        };

        if (ProxySet)
        {
            _httpHandler.Proxy = Proxy;
            _httpHandler.UseProxy = true;
        }

        _httpClient = new HttpClient(_httpHandler);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", AppBuildInfoService.Version);
    }

    private async Task SetProxy()
    {
        if (!string.IsNullOrEmpty(_startupArgsService.LaunchArguments?.ProxyUrl))
            ProxyUrl = _startupArgsService.LaunchArguments.ProxyUrl;

        if (string.IsNullOrEmpty(ProxyUrl))
        {
            var proxyUrl = _appStorageService.Get("VRCX_ProxyServer");
            if (!string.IsNullOrEmpty(proxyUrl))
                ProxyUrl = proxyUrl;
        }

        if (string.IsNullOrEmpty(ProxyUrl))
            return;

        try
        {
            ProxySet = true;
            Proxy = new WebProxy(ProxyUrl);
        }
        catch (UriFormatException)
        {
            _appStorageService.Set("VRCX_ProxyServer", string.Empty);
            _appStorageService.Save();
            const string message =
                "The proxy server URI you used is invalid.\nVRCX will close, please correct the proxy URI.";
            Logger.Error(message);
            await _messageBoxService.ShowAsync(message, "Invalid Proxy URI", NativeMessageBoxIcon.Error);
            _ = _platformLifetimeService.InvokeShutdownAsync().AsTask();
        }
    }

    public void ClearCookies()
    {
        // TODO: Delete cookies for WebView
        CookieContainer = new CookieContainer();
        InitializeHttpClient();
        SaveCookies();
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
            using var stream = new MemoryStream(Convert.FromBase64String((string)item[0]));
            CookieContainer = new CookieContainer();
            CookieContainer.Add(System.Text.Json.JsonSerializer.Deserialize<CookieCollection>(stream));
            InitializeHttpClient();
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to load cookies");
        }
    }

    private List<Cookie> GetAllCookies()
    {
        return CookieContainer.GetAllCookies().ToList();
    }

    public void SaveCookies()
    {
        if (!_cookieDirty)
            return;

        try
        {
            var cookies = GetAllCookies();
            using var memoryStream = new MemoryStream();
            System.Text.Json.JsonSerializer.Serialize(memoryStream, cookies);
            _sqliteService.ExecuteNonQuery(
                "INSERT OR REPLACE INTO `cookies` (`key`, `value`) VALUES (@key, @value)",
                new Dictionary<string, object>()
                {
                    { "@key", "default" },
                    { "@value", Convert.ToBase64String(memoryStream.ToArray()) }
                }
            );

            _cookieDirty = false;
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to save cookies");
        }
    }

    public string GetCookies()
    {
        _cookieDirty = true; // force cookies to be saved for lastUserLoggedIn

        using var memoryStream = new MemoryStream();
        System.Text.Json.JsonSerializer.Serialize(memoryStream, GetAllCookies());
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public void SetCookies(string cookies)
    {
        using (var stream = new MemoryStream(Convert.FromBase64String(cookies)))
        {
            CookieContainer.Add(System.Text.Json.JsonSerializer.Deserialize<CookieCollection>(stream));
        }

        _cookieDirty = true; // force cookies to be saved for lastUserLoggedIn
    }

    private async Task<HttpRequestMessage> BuildLegacyImageUploadRequest(string url,
        IDictionary<string, object> options)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
        var content = new MultipartFormDataContent(boundary);

        if (options.TryGetValue("postData", out var postDataObject))
        {
            content.Add(new StringContent((string)postDataObject), "data");
        }

        var imageData = options["imageData"] as string;
        var fileToUpload = ImageUtils.ResizeImageToFitLimits(Convert.FromBase64String(imageData), false);
        var imageContent = new ByteArrayContent(fileToUpload);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(imageContent, "image", "image.png");

        request.Content = content;
        return request;
    }

    private async Task<HttpRequestMessage> BuildUploadFilePutRequest(string url, IDictionary<string, object> options)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, url);
        var fileData = options["fileData"] as string;
        var sentData = Convert.FromBase64CharArray(fileData.ToCharArray(), 0, fileData.Length);
        var content = new ByteArrayContent(sentData);
        content.Headers.ContentType = new MediaTypeHeaderValue(options["fileMIME"] as string);
        if (options.TryGetValue("fileMD5", out var fileMd5))
            content.Headers.ContentMD5 = Convert.FromBase64String(fileMd5 as string);
        request.Content = content;
        return request;
    }

    private async Task<HttpRequestMessage> BuildImageUploadRequest(string url, IDictionary<string, object> options)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
        var content = new MultipartFormDataContent(boundary);

        if (options.TryGetValue("postData", out var postDataObject))
        {
            var jsonPostData = (JObject)JsonConvert.DeserializeObject((string)postDataObject);
            if (jsonPostData != null)
            {
                foreach (var data in jsonPostData)
                {
                    content.Add(new StringContent(data.Value?.ToString() ?? string.Empty), data.Key);
                }
            }
        }

        var imageData = options["imageData"] as string;
        var matchingDimensions = options["matchingDimensions"] as bool? ?? false;
        var fileToUpload = ImageUtils.ResizeImageToFitLimits(Convert.FromBase64String(imageData), matchingDimensions);

        var imageContent = new ByteArrayContent(fileToUpload);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(imageContent, "file", "blob");

        request.Content = content;
        return request;
    }

    private async Task<HttpRequestMessage> BuildPrintImageUploadRequest(string url, IDictionary<string, object> options)
    {
        if (options.TryGetValue("cropWhiteBorder", out var cropWhiteBorder) && (bool)cropWhiteBorder)
        {
            var oldImageData = options["imageData"] as string;
            var ms = new MemoryStream(Convert.FromBase64String(oldImageData));
            var print = await Image.LoadAsync(ms);
            if (ImageUtils.CropPrint(ref print))
            {
                var ms2 = new MemoryStream();
                await print.SaveAsPngAsync(ms2);
                options["imageData"] = Convert.ToBase64String(ms2.ToArray());
            }
        }

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
        var content = new MultipartFormDataContent(boundary);

        var imageData = options["imageData"] as string;
        var fileToUpload = ImageUtils.ResizePrintImage(Convert.FromBase64String(imageData));

        var imageContent = new ByteArrayContent(fileToUpload);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        imageContent.Headers.ContentLength = fileToUpload.Length;
        content.Add(imageContent, "image", "image");

        if (options.TryGetValue("postData", out var postDataObject))
        {
            var jsonPostData = JsonConvert.DeserializeObject<Dictionary<string, string>>(postDataObject.ToString());
            if (jsonPostData != null)
            {
                foreach (var (key, value) in jsonPostData)
                {
                    var stringContent = new StringContent(value, Encoding.UTF8, "text/plain");
                    content.Add(stringContent, key);
                }
            }
        }

        request.Content = content;
        return request;
    }

    public async Task<string> ExecuteJson(string options)
    {
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(options);
        var result = await Execute(data);
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            status = result.Item1,
            message = result.Item2
        });
    }

    public async Task<Tuple<int, string>> Execute(IDictionary<string, object> options)
    {
        // TODO: add scope logging, but refactor this api first
        try
        {
            var url = (string)options["url"];
            HttpRequestMessage request;

            // Handle special upload types
            if (options.TryGetValue("uploadImageLegacy", out _))
            {
                request = await BuildLegacyImageUploadRequest(url, options);
            }
            else if (options.TryGetValue("uploadFilePUT", out _))
            {
                request = await BuildUploadFilePutRequest(url, options);
            }
            else if (options.TryGetValue("uploadImage", out _))
            {
                request = await BuildImageUploadRequest(url, options);
            }
            else if (options.TryGetValue("uploadImagePrint", out _))
            {
                request = await BuildPrintImageUploadRequest(url, options);
            }
            else
            {
                // Standard request
                var httpMethod = HttpMethod.Get;
                if (options.TryGetValue("method", out var methodObj))
                {
                    httpMethod = HttpMethod.Parse(methodObj.ToString());
                }

                request = new HttpRequestMessage(httpMethod, url);

                // Handle body for non-GET requests
                if (httpMethod != HttpMethod.Get && options.TryGetValue("body", out var body))
                {
                    var bodyContent = new StringContent((string)body, Encoding.UTF8);

                    // Set content type if specified in headers
                    if (options.TryGetValue("headers", out var headersObj))
                    {
                        var headersDict = ParseHeaders(headersObj);
                        if (headersDict.TryGetValue("Content-Type", out var contentType))
                        {
                            bodyContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                        }
                    }

                    request.Content = bodyContent;
                }
            }

            // Apply headers
            if (options.TryGetValue("headers", out var headers))
            {
                var headersDict = ParseHeaders(headers);
                foreach (var (key, value) in headersDict)
                {
                    // Skip Content-Type as it's set on content
                    if (string.Equals(key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (string.Equals(key, "Referer", StringComparison.OrdinalIgnoreCase))
                    {
                        request.Headers.Referrer = new Uri(value);
                    }
                    else
                    {
                        request.Headers.TryAddWithoutValidation(key, value);
                    }
                }
            }

            using var response = await _httpClient.SendAsync(request);

            // Check if cookies were modified
            if (response.Headers.Contains("Set-Cookie"))
                _cookieDirty = true;

            var contentTypeResponse = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

            if (contentTypeResponse.Contains("image/") || contentTypeResponse.Contains("application/octet-stream"))
            {
                // Base64 response data for image
                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                return new Tuple<int, string>(
                    (int)response.StatusCode,
                    $"data:image/png;base64,{Convert.ToBase64String(imageBytes)}"
                );
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            return new Tuple<int, string>(
                (int)response.StatusCode,
                responseBody
            );
        }
        catch (HttpRequestException httpException)
        {
            if (httpException.InnerException != null)
                Logger.Error(httpException, "An HTTP error occurred while executing web request");

            // Try to get status code if available
            var statusCode = httpException.StatusCode.HasValue ? (int)httpException.StatusCode.Value : -1;

            return new Tuple<int, string>(
                statusCode,
                httpException.Message
            );
        }
        catch (Exception e)
        {
            if (e.InnerException != null)
                Logger.Error(e, "An error occurred while executing web request");

            return new Tuple<int, string>(
                -1,
                e.Message
            );
        }
    }

    private static Dictionary<string, string> ParseHeaders(object headers)
    {
        Dictionary<string, string> headersDict;
        if (headers.GetType() == typeof(JObject))
        {
            headersDict = ((JObject)headers).ToObject<Dictionary<string, string>>();
        }
        else
        {
            var headersKvp = (IEnumerable<KeyValuePair<string, object>>)headers;
            headersDict = new Dictionary<string, string>();
            foreach (var (key, value) in headersKvp)
                headersDict.Add(key, value.ToString());
        }

        return headersDict;
    }

    public void Dispose()
    {
        _timer.Dispose();
        _httpClient?.Dispose();
        _httpHandler?.Dispose();
    }
}