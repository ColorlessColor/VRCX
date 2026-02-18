using System.Text.Json.Serialization;

namespace VRCX.App.WebViewInterop;

[JsonSerializable(typeof(WebViewJsonIpcRequest))]
[JsonSerializable(typeof(WebViewMessage))]
[JsonSerializable(typeof(WebViewMessage<WebViewJsonIpcRequest>))]
[JsonSerializable(typeof(WebViewJsonIpcResponseMessage))]
[JsonSerializable(typeof(WebViewJsonIpcRequest))]
[JsonSerializable(typeof(WebViewJsonIpcResponse))]
[JsonSerializable(typeof(WebViewJsonIpcResponseError))]
[JsonSerializable(typeof(WebViewJsonIpcErrorMessage))]
[JsonSerializable(typeof(WebViewJsonIpcError))]
public sealed partial class WebViewMessageJsonContext : JsonSerializerContext;