using System.Net;
using System.Text.Json.Serialization;

namespace VRCX.Core.Models.WebApi;

[JsonSerializable(typeof(WebApiRequestBase))]
[JsonSerializable(typeof(WebApiRequest))]
[JsonSerializable(typeof(WebApiUploadImageLegacyRequest))]
[JsonSerializable(typeof(WebApiUploadFilePutRequest))]
[JsonSerializable(typeof(WebApiUploadImageRequest))]
[JsonSerializable(typeof(WebApiUploadImagePrintRequest))]
[JsonSerializable(typeof(WebApiResponse))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(CookieCollection))]
[JsonSerializable(typeof(List<Cookie>))]
internal sealed partial class WebApiJsonContext : JsonSerializerContext;