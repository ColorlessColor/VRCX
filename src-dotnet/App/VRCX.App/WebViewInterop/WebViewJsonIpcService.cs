using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Serilog;
using Serilog.Context;
using VRCX.Core.Models.GamePlayerPrefs;
using VRCX.Core.WebViewInterop;

namespace VRCX.App.WebViewInterop;

public class WebViewJsonIpcService
{
    private readonly ILogger _logger = Log.ForContext<WebViewJsonIpcService>();

    private readonly Dictionary<string, object> _jsonIpcObjects = new();

    public async Task<string> HandleJsonIpcMessage(string messageRaw)
    {
        try
        {
            var jsonDoc = JsonNode.Parse(messageRaw);
            if (jsonDoc is null)
                throw new ArgumentException("Json message is null", nameof(messageRaw));

            if (jsonDoc["type"] is not { } typeNode)
                throw new ArgumentException("Json message didn't have type field", nameof(messageRaw));

            var type = typeNode.GetValue<string>();
            if (type != "InvokeJsonIpcMethod")
                throw new NotSupportedException("Json message type not supported: " + type);

            var request =
                jsonDoc.Deserialize<WebViewMessage<WebViewJsonIpcRequest>>(
                    WebViewMessageJsonContext.Default
                        .WebViewMessageWebViewJsonIpcRequest
                );
            if (request is null)
                throw new InvalidOperationException(
                    "Deserialization of WebViewJsonIpcRequest is null after check, it should never be null here.");

            using (LogContext.PushProperty("JsonIpcClassName", request.Data.ClassName))
            using (LogContext.PushProperty("JsonIpcMethodName", request.Data.MethodName))
            using (LogContext.PushProperty("JsonIpcRequestId", request.Data.RequestId))
            {
                try
                {
                    var resultJson = await InvokeJsonIpcMethod(
                        request.Data.ClassName,
                        request.Data.MethodName,
                        request.Data.ArgsJson
                    );

                    var response = new WebViewJsonIpcResponseMessage(new WebViewJsonIpcResponse(
                        RequestId: request.Data.RequestId,
                        ResultJson: resultJson,
                        IsError: false,
                        Error: null
                    ));

                    return JsonSerializer.Serialize(response,
                        WebViewMessageJsonContext.Default.WebViewJsonIpcResponseMessage);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error invoking JSON IPC method: {Message}", messageRaw);

                    var errorResponse = new WebViewJsonIpcResponseMessage(new WebViewJsonIpcResponse(
                        RequestId: request.Data.RequestId,
                        ResultJson: string.Empty,
                        IsError: true,
                        Error: new WebViewJsonIpcResponseError(Exception: ex.ToString())
                    ));

                    return JsonSerializer.Serialize(errorResponse,
                        WebViewMessageJsonContext.Default.WebViewJsonIpcResponseMessage);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error handling JSON IPC message: {Message}", messageRaw);

            var errorResponse = new WebViewJsonIpcErrorMessage(new WebViewJsonIpcError(
                Error: new WebViewJsonIpcResponseError(Exception: ex.ToString())
            ));

            return JsonSerializer.Serialize(
                errorResponse,
                WebViewMessageJsonContext.Default.WebViewJsonIpcErrorMessage
            );
        }
    }

    public void RegisterJsonIpcObject(string name, object obj,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.AllMethods |
                                    DynamicallyAccessedMemberTypes.NonPublicPropertiesWithInherited)]
        Type objectType)
    {
        _jsonIpcObjects[name] = obj;
    }

    private async ValueTask<string> InvokeJsonIpcMethod(string objectName, string methodName, string jsonArgs)
    {
        if (_jsonIpcObjects.TryGetValue(objectName, out var obj))
        {
            var argsJsonNode = JsonNode.Parse(jsonArgs);
            if (argsJsonNode is null)
                throw new ArgumentNullException(nameof(jsonArgs),
                    "Deserialization of JSON IPC method arguments resulted in null.");

            if (argsJsonNode.GetValueKind() != JsonValueKind.Array)
                throw new ArgumentException("JSON IPC method arguments are not an array.");

            var deserializedArgs = argsJsonNode.AsArray();

            var method = obj.GetType().GetMethods()
                .Where(mi => mi.Name == methodName)
                .FirstOrDefault(mi => mi.GetParameters().Length == deserializedArgs.Count);
            if (method == null)
                throw new MissingMethodException(
                    $"Method '{methodName}' with {deserializedArgs.Count} args not found on object '{objectName}'.");

            var parameters = method.GetParameters();
            var args = new object?[parameters.Length];
            if (parameters.Length > 0)
            {
                var argTypes = parameters.Select(p => p.ParameterType).ToArray();

                for (var i = 0; i < parameters.Length; i++)
                {
                    args[i] = deserializedArgs[i]
                        .Deserialize(argTypes[i], WebViewInteropJsonContext.Default);
                }
            }

            var result = method.Invoke(obj, args);
            if (result is Task task)
            {
                result = await AwaitAndGetResult(task, method.ReturnType);
            }

            var jsonResult =
                result is not null
                    ? JsonSerializer.Serialize(result, WebViewInteropJsonContext.Default.GetTypeInfo(result.GetType()))
                    : "null";

            return jsonResult;
        }

        throw new KeyNotFoundException($"Object '{objectName}' not registered.");
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(Task<string>))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(Task<string?>))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(Task<bool>))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(Task<int>))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(Task<double>))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(Dictionary<string, RegistryKeyValue>))]
    public static async Task<object?> AwaitAndGetResult(Task task, Type taskType)
    {
        await task.ConfigureAwait(false);

        // taskType is something like typeof(Task<string>)
        // Extract T from Task<T>
        if (taskType.IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            return taskType.GetProperty("Result")!.GetValue(task);
        }

        // It's a plain Task (no result)
        return null;
    }
}