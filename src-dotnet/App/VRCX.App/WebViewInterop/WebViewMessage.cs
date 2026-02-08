using System.Text.Json.Serialization;

namespace VRCX.App.WebViewInterop;

public record WebViewMessage(
    [property: JsonPropertyName("type")] string Type
);

public record WebViewMessage<T>(
    string Type,
    [property: JsonPropertyName("data")] T Data
) : WebViewMessage(Type);

public sealed record WebViewJsonIpcRequest(
    [property: JsonPropertyName("requestId")]
    string RequestId,
    [property: JsonPropertyName("className")]
    string ClassName,
    [property: JsonPropertyName("methodName")]
    string MethodName,
    [property: JsonPropertyName("argsJson")]
    string ArgsJson
);

public sealed record WebViewJsonIpcResponseMessage(
    WebViewJsonIpcResponse Data)
    : WebViewMessage<WebViewJsonIpcResponse>("WebViewJsonIpcResponse", Data);

public sealed record WebViewJsonIpcResponse(
    [property: JsonPropertyName("requestId")]
    string RequestId,
    [property: JsonPropertyName("resultJson")]
    string ResultJson,
    [property: JsonPropertyName("isError")]
    bool IsError,
    [property: JsonPropertyName("error")] WebViewJsonIpcResponseError? Error
);

public sealed record WebViewJsonIpcResponseError(
    [property: JsonPropertyName("exception")]
    string Exception
);

public sealed record WebViewJsonIpcErrorMessage(
    WebViewJsonIpcError Data)
    : WebViewMessage<WebViewJsonIpcError>("WebViewJsonIpcError", Data);

public sealed record WebViewJsonIpcError(
    [property: JsonPropertyName("error")] WebViewJsonIpcResponseError Error
);