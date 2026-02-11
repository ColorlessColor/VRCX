using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Serilog;
using Serilog.Context;
using SixLabors.ImageSharp;
using VRCX.Core.Models.WebApi;
using VRCX.Core.Utils;

namespace VRCX.Core.Services;

public sealed class WebApiService : IDisposable
{
    private static readonly ILogger Logger = Log.ForContext<WebApiService>();

    private readonly CookieContainer _cookieContainer = new();

    // TODO: Replace these with custom http DelegatingHandler
    private bool _cookieDirty;
    private readonly Timer _timer;

    private HttpClient? _httpClient;
    private SocketsHttpHandler? _httpHandler;

    private readonly AppWebProxy _appWebProxy;
    private readonly SqliteService _sqliteService;

    public WebApiService(SqliteService sqliteService, AppWebProxy appWebProxy)
    {
        _sqliteService = sqliteService;
        _appWebProxy = appWebProxy;

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
        InitializeHttpClient();
        LoadCookies();
        _timer.Change(1000, 1000);
    }

    private void InitializeHttpClient()
    {
        _httpClient?.Dispose();

        _httpHandler = new SocketsHttpHandler
        {
            CookieContainer = _cookieContainer,
            UseCookies = true,
            AutomaticDecompression = DecompressionMethods.All,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            MaxConnectionsPerServer = 10,
            Proxy = _appWebProxy,
            UseProxy = true
        };

        _httpClient = new HttpClient(_httpHandler);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", AppBuildInfoService.Version);
    }

    public void ClearCookies()
    {
        // TODO: Delete cookies for WebView
        foreach (Cookie cookie in _cookieContainer.GetAllCookies())
        {
            cookie.Expired = true;
        }

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
            _cookieContainer.Add(JsonSerializer.Deserialize<CookieCollection>(stream));
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to load cookies");
        }
    }

    private List<Cookie> GetAllCookies()
    {
        return _cookieContainer.GetAllCookies().ToList();
    }

    public void SaveCookies()
    {
        if (!_cookieDirty)
            return;

        try
        {
            var cookies = GetAllCookies();
            using var memoryStream = new MemoryStream();
            JsonSerializer.Serialize(memoryStream, cookies);
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
        JsonSerializer.Serialize(memoryStream, GetAllCookies());
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public void SetCookies(string cookies)
    {
        using (var stream = new MemoryStream(Convert.FromBase64String(cookies)))
        {
            _cookieContainer.Add(JsonSerializer.Deserialize<CookieCollection>(stream));
        }

        _cookieDirty = true; // force cookies to be saved for lastUserLoggedIn
    }

    private static HttpRequestMessage BuildLegacyImageUploadRequest(WebApiUploadImageLegacyRequest requestPayload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestPayload.Url);
        var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
        var content = new MultipartFormDataContent(boundary);

        if (requestPayload.PostData is { } postData)
        {
            content.Add(new StringContent(postData), "data");
        }

        var imageData = requestPayload.ImageData;
        var fileToUpload = ImageUtils.ResizeImageToFitLimits(Convert.FromBase64String(imageData), false);

        var imageContent = new ByteArrayContent(fileToUpload);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(imageContent, "image", "image.png");

        request.Content = content;
        return request;
    }

    private static HttpRequestMessage BuildUploadFilePutRequest(WebApiUploadFilePutRequest requestPayload)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, requestPayload.Url);

        var fileData = requestPayload.FileData;
        var sentData = Convert.FromBase64CharArray(fileData.ToCharArray(), 0, fileData.Length);

        var content = new ByteArrayContent(sentData);
        content.Headers.ContentType = new MediaTypeHeaderValue(requestPayload.FileMime);

        if (requestPayload.FileMd5 is { } fileMd5)
            content.Headers.ContentMD5 = Convert.FromBase64String(fileMd5);

        request.Content = content;
        return request;
    }

    private static HttpRequestMessage BuildImageUploadRequest(WebApiUploadImageRequest uploadImageRequest)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uploadImageRequest.Url);
        var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
        var content = new MultipartFormDataContent(boundary);

        if (uploadImageRequest.PostData is { } postData)
        {
            var jsonPostData = JsonSerializer.Deserialize<Dictionary<string, string>>(postData);
            if (jsonPostData != null)
            {
                foreach (var data in jsonPostData)
                {
                    content.Add(new StringContent(data.Value), data.Key);
                }
            }
        }

        var imageData = uploadImageRequest.ImageData;
        var matchingDimensions = uploadImageRequest.MatchingDimensions;
        var fileToUpload = ImageUtils.ResizeImageToFitLimits(Convert.FromBase64String(imageData), matchingDimensions);

        var imageContent = new ByteArrayContent(fileToUpload);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(imageContent, "file", "blob");

        request.Content = content;
        return request;
    }

    private async ValueTask<HttpRequestMessage> BuildPrintImageUploadRequestAsync(
        WebApiUploadImagePrintRequest uploadImagePrintRequest)
    {
        var fileToUpload = await ProcessPrintImageDataAsync(
            uploadImagePrintRequest.ImageData,
            uploadImagePrintRequest.CropWhiteBorder
        );

        var request = new HttpRequestMessage(HttpMethod.Post, uploadImagePrintRequest.Url);
        var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
        var content = new MultipartFormDataContent(boundary);

        var imageContent = new ByteArrayContent(fileToUpload);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        imageContent.Headers.ContentLength = fileToUpload.Length;
        content.Add(imageContent, "image", "image");

        if (uploadImagePrintRequest.PostData is { } postData)
        {
            var jsonPostData = JsonSerializer.Deserialize<Dictionary<string, string>>(postData);
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

    private async ValueTask<byte[]> ProcessPrintImageDataAsync(string imageAsBase64, bool cropWhiteBoard)
    {
        var imageBytes = Convert.FromBase64String(imageAsBase64);

        if (cropWhiteBoard)
        {
            using var imageStream = new MemoryStream(imageBytes);
            using var print = await Image.LoadAsync(imageStream);
            if (ImageUtils.CropPrint(print))
            {
                using var cropResultStream = new MemoryStream();
                await print.SaveAsPngAsync(cropResultStream);

                imageBytes = cropResultStream.ToArray();
            }
        }

        return ImageUtils.ResizePrintImage(imageBytes);
    }

    public async Task<string> ExecuteJson(string requestJson)
    {
        WebApiRequestBase request;
        try
        {
            request = ParseRequestJson(requestJson);
        }
        catch (Exception e)
        {
            Logger.Error(e, "Failed to parse web api request json: {RequestJson}", requestJson);
            throw;
        }

        var requestType = request.GetType().Name;
        var requestMethod = request is WebApiRequest stdRequest ? stdRequest.Method : requestType;

        using (LogContext.PushProperty("RequestUrl", request.Url))
        using (LogContext.PushProperty("RequestType", requestType))
        using (LogContext.PushProperty("RequestMethod", requestMethod))
        {
            Logger.Verbose("Executing web api request {RequestMethod} {RequestUrl}",
                requestMethod,
                request.Url
            );

            try
            {
                var response = await ExecuteCoreAsync(request);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error executing web api request {RequestMethod} {RequestUrl}",
                    requestMethod,
                    request.Url);

                return JsonSerializer.Serialize(new WebApiResponse(-1, ex.Message));
            }
        }
    }

    private static WebApiRequestBase ParseRequestJson(string requestJson)
    {
        var jsonDoc = JsonDocument.Parse(requestJson);
        if (jsonDoc.RootElement.Deserialize<WebApiRequestBase>() is not { } requestBase)
        {
            throw new ArgumentException("WebApi json request are null json", nameof(requestJson));
        }

        if (requestBase.IsUploadImageLegacy)
        {
            return jsonDoc.RootElement.Deserialize<WebApiUploadImageLegacyRequest>() ??
                   throw new ArgumentException(
                       "WebApi json request are invalid for WebApiUploadImageLegacyRequest",
                       nameof(requestJson)
                   );
        }

        if (requestBase.IsUploadFilePut)
        {
            return jsonDoc.RootElement.Deserialize<WebApiUploadFilePutRequest>() ??
                   throw new ArgumentException(
                       "WebApi json request are invalid for WebApiUploadFilePutRequest",
                       nameof(requestJson)
                   );
        }

        if (requestBase.IsUploadImage)
        {
            return jsonDoc.RootElement.Deserialize<WebApiUploadImageRequest>() ??
                   throw new ArgumentException(
                       "WebApi json request are invalid for WebApiUploadImageRequest",
                       nameof(requestJson)
                   );
        }

        if (requestBase.IsUploadImagePrint)
        {
            return jsonDoc.RootElement.Deserialize<WebApiUploadImagePrintRequest>() ??
                   throw new ArgumentException(
                       "WebApi json request are invalid for WebApiUploadImagePrintRequest",
                       nameof(requestJson)
                   );
        }

        return jsonDoc.RootElement.Deserialize<WebApiRequest>() ??
               throw new ArgumentException(
                   "WebApi json request are invalid for WebApiRequestWithBody",
                   nameof(requestJson)
               );
    }

    private async ValueTask<WebApiResponse> ExecuteCoreAsync(WebApiRequestBase webApiRequestBase)
    {
        try
        {
            var url = webApiRequestBase.Url;
            HttpRequestMessage request;

            switch (webApiRequestBase)
            {
                case WebApiUploadImageLegacyRequest uploadImageLegacyRequest:
                    request = BuildLegacyImageUploadRequest(uploadImageLegacyRequest);
                    break;
                case WebApiUploadFilePutRequest uploadFilePutRequest:
                    request = BuildUploadFilePutRequest(uploadFilePutRequest);
                    break;
                case WebApiUploadImageRequest uploadImageRequest:
                    request = BuildImageUploadRequest(uploadImageRequest);
                    break;
                case WebApiUploadImagePrintRequest uploadImagePrintRequest:
                    request = await BuildPrintImageUploadRequestAsync(uploadImagePrintRequest);
                    break;
                case WebApiRequest stdRequest:
                    // Standard request
                    var httpMethod = HttpMethod.Parse(stdRequest.Method);
                    request = new HttpRequestMessage(httpMethod, url);

                    // Handle body for non-GET requests
                    if (httpMethod != HttpMethod.Get && stdRequest.Body is { } body)
                    {
                        var bodyContent = new StringContent(body, Encoding.UTF8);

                        // Set content type if specified in headers
                        if (stdRequest.Headers?
                                .FirstOrDefault(h => h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                                .Value is { } contentType)
                        {
                            bodyContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                        }

                        request.Content = bodyContent;
                    }

                    break;
                default:
                    throw new ArgumentException(
                        "Unsupported WebApiRequestBase type: " + webApiRequestBase.GetType().FullName,
                        nameof(webApiRequestBase));
            }

            // Apply headers
            if (webApiRequestBase.Headers is { } headers)
            {
                foreach (var (key, value) in headers)
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
                return new WebApiResponse(
                    (int)response.StatusCode,
                    $"data:image/png;base64,{Convert.ToBase64String(imageBytes)}"
                );
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            return new WebApiResponse(
                (int)response.StatusCode,
                responseBody
            );
        }
        catch (HttpRequestException httpException)
        {
            Logger.Error(httpException, "An HTTP error occurred while executing web request");

            // Try to get status code if available
            var statusCode = httpException.StatusCode.HasValue ? (int)httpException.StatusCode.Value : -1;

            return new WebApiResponse(statusCode, httpException.Message);
        }
        catch (Exception e)
        {
            Logger.Error(e, "An error occurred while executing web request");

            return new WebApiResponse(-1, e.Message);
        }
    }

    public void Dispose()
    {
        _timer.Dispose();
        _httpClient?.Dispose();
        _httpHandler?.Dispose();
    }
}