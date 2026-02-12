using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Serilog;
using Serilog.Context;
using VRCX.Core.Models.WebApi;

namespace VRCX.Core.Services.WebApi;

public sealed partial class WebApiService : IDisposable
{
    private readonly ILogger _logger = Log.ForContext<WebApiService>();

    private readonly CookieContainer _cookieContainer = new();
    private readonly HttpClient _httpClient;

    private readonly SqliteService _sqliteService;

    public WebApiService(SqliteService sqliteService, AppWebProxy appWebProxy)
    {
        _sqliteService = sqliteService;

        _httpClient = new HttpClient(
            new WebApiHttpHandler(OnAfterHttpResponse)
            {
                InnerHandler = new SocketsHttpHandler
                {
                    CookieContainer = _cookieContainer,
                    UseCookies = true,
                    AutomaticDecompression = DecompressionMethods.All,
                    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                    MaxConnectionsPerServer = 10,
                    Proxy = appWebProxy,
                    UseProxy = true
                }
            });

        _httpClient.DefaultRequestHeaders.Add("User-Agent", AppBuildInfoService.Version);
    }

    public void Init()
    {
        LoadCookies();
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
            _logger.Error(e, "Failed to parse web api request json: {RequestJson}", requestJson);
            throw;
        }

        var requestType = request.GetType().Name;
        var requestMethod = request is WebApiRequest stdRequest ? stdRequest.Method : requestType;

        using (LogContext.PushProperty("RequestUrl", request.Url))
        using (LogContext.PushProperty("RequestType", requestType))
        using (LogContext.PushProperty("RequestMethod", requestMethod))
        {
            _logger.Verbose("Executing web api request {RequestMethod} {RequestUrl}",
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
                _logger.Error(ex, "Error executing web api request {RequestMethod} {RequestUrl}",
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
                    request = WebApiService.BuildLegacyImageUploadRequest(uploadImageLegacyRequest);
                    break;
                case WebApiUploadFilePutRequest uploadFilePutRequest:
                    request = WebApiService.BuildUploadFilePutRequest(uploadFilePutRequest);
                    break;
                case WebApiUploadImageRequest uploadImageRequest:
                    request = WebApiService.BuildImageUploadRequest(uploadImageRequest);
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
            _logger.Error(httpException, "An HTTP error occurred while executing web request");

            // Try to get status code if available
            var statusCode = httpException.StatusCode.HasValue ? (int)httpException.StatusCode.Value : -1;

            return new WebApiResponse(statusCode, httpException.Message);
        }
        catch (Exception e)
        {
            _logger.Error(e, "An error occurred while executing web request");

            return new WebApiResponse(-1, e.Message);
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}