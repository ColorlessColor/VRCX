using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace VRCX.App.Ipc;

[ClassInterface(ClassInterfaceType.AutoDual)]
[ComVisible(true)]
public class WebViewJsonIpcInterface(Func<string, string, string, ValueTask<string>> invokeJsonIpcMethod)
{
    public async Task<string> InvokeJsonIpcMethod(string objectName, string methodName, string jsonArgs)
    {
        return await invokeJsonIpcMethod(objectName, methodName, jsonArgs);
    }
}