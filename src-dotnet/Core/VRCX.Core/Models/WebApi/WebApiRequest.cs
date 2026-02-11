using System.Text.Json.Serialization;

namespace VRCX.Core.Models.WebApi;

public record WebApiRequestBase(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("headers")]
    Dictionary<string, string>? Headers = null,
    [property: JsonPropertyName("uploadImageLegacy")]
    bool IsUploadImageLegacy = false,
    [property: JsonPropertyName("uploadFilePUT")]
    bool IsUploadFilePut = false,
    [property: JsonPropertyName("uploadImage")]
    bool IsUploadImage = false,
    [property: JsonPropertyName("uploadImagePrint")]
    bool IsUploadImagePrint = false
);

public sealed record WebApiRequest(
    string Url,
    Dictionary<string, string>? Headers,
    [property: JsonPropertyName("method")] string Method = "GET",
    [property: JsonPropertyName("body")] string? Body = null
) : WebApiRequestBase(Url, Headers);

public sealed record WebApiUploadImageLegacyRequest(
    string Url,
    Dictionary<string, string>? Headers,
    [property: JsonPropertyName("imageData")]
    string ImageData,
    [property: JsonPropertyName("postData")]
    string? PostData = null
) : WebApiRequestBase(Url, Headers, IsUploadImageLegacy: true);

public sealed record WebApiUploadFilePutRequest(
    string Url,
    Dictionary<string, string>? Headers,
    [property: JsonPropertyName("fileData")]
    string FileData,
    [property: JsonPropertyName("fileMIME")]
    string FileMime,
    [property: JsonPropertyName("fileMD5")]
    string? FileMd5 = null
) : WebApiRequestBase(Url, Headers, IsUploadFilePut: true);

public sealed record WebApiUploadImageRequest(
    string Url,
    Dictionary<string, string>? Headers,
    [property: JsonPropertyName("imageData")]
    string ImageData,
    [property: JsonPropertyName("postData")]
    string? PostData = null,
    [property: JsonPropertyName("matchingDimensions")]
    bool MatchingDimensions = false
) : WebApiRequestBase(Url, Headers, IsUploadImage: true);

public sealed record WebApiUploadImagePrintRequest(
    string Url,
    Dictionary<string, string>? Headers,
    [property: JsonPropertyName("imageData")]
    string ImageData,
    [property: JsonPropertyName("postData")]
    string? PostData = null,
    [property: JsonPropertyName("cropWhiteBorder")]
    bool CropWhiteBorder = false
) : WebApiRequestBase(Url, Headers, IsUploadImagePrint: true);