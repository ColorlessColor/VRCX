using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Context;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace VRCX.App.WebViewInterop;

public class WebViewJsonIpcService
{
    private readonly ILogger _logger = Log.ForContext<WebViewJsonIpcService>();

    public readonly WebViewJsonIpcInterface IpcHostObject;

    private readonly Dictionary<string, object> _jsonIpcObjects = new();

    public WebViewJsonIpcService()
    {
        IpcHostObject = new WebViewJsonIpcInterface(InvokeJsonIpcMethod);
    }

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

            var request = jsonDoc.Deserialize<WebViewMessage<WebViewJsonIpcRequest>>();
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

                    return JsonSerializer.Serialize(response);
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

                    return JsonSerializer.Serialize(errorResponse);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error handling JSON IPC message: {Message}", messageRaw);

            var errorResponse = new WebViewJsonIpcErrorMessage(new WebViewJsonIpcError(
                Error: new WebViewJsonIpcResponseError(Exception: ex.ToString())
            ));

            return JsonSerializer.Serialize(errorResponse);
        }
    }

    public void RegisterJsonIpcObject(string name, object obj)
    {
        _jsonIpcObjects[name] = obj;
    }

    private async ValueTask<string> InvokeJsonIpcMethod(string objectName, string methodName, string jsonArgs)
    {
        if (_jsonIpcObjects.TryGetValue(objectName, out var obj))
        {
            var deserializedArgs = JArray.Parse(jsonArgs);
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
                    args[i] = deserializedArgs[i].ToObject(GetNullableType(argTypes[i]));
                }
            }

            var result = method.Invoke(obj, args);
            if (result is Task task)
            {
                result = await RunAnyTask(task);
            }

            var jsonResult = JsonConvert.SerializeObject(result);
            return jsonResult;
        }

        throw new KeyNotFoundException($"Object '{objectName}' not registered.");
    }

    private static Type GetNullableType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsValueType ? typeof(Nullable<>).MakeGenericType(type) : type;
    }

    private static async Task<object?> RunAnyTask(Task task)
    {
        await task;
        var voidTaskType = typeof(Task<>).MakeGenericType(Type.GetType("System.Threading.Tasks.VoidTaskResult"));

        if (voidTaskType.IsInstanceOfType(task))
            return null;

        var property = task.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance);
        return property?.GetValue(task);
    }
}