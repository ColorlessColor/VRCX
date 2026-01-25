using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace VRCX.Core.Services.Platform;

public interface IMainWebViewService
{
    Task ExecuteScriptAsync(string script);

    Task ExecuteScriptAsync(string methodName, params object[] args)
    {
         var argsJson = JsonSerializer.Serialize(args);
         var wrappedScript = $"{methodName}(...{argsJson})";
         return ExecuteScriptAsync(wrappedScript);
    }
}