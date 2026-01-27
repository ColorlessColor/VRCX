using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VRCX.App.Ipc;

public class WebViewJsonIpcService
{
    public readonly WebViewJsonIpcInterface IpcHostObject;
    
    private readonly Dictionary<string, object> _jsonIpcObjects = new();

    public WebViewJsonIpcService()
    {
        IpcHostObject = new WebViewJsonIpcInterface(InvokeJsonIpcMethod);
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
                throw new MissingMethodException($"Method '{methodName}' with {deserializedArgs.Count} args not found on object '{objectName}'.");

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