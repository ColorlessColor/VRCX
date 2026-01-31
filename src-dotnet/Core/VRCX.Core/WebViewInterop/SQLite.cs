using System.Text.Json;
using VRCX.Core.Services;

namespace VRCX.Core.WebViewInterop;

public class SQLite(SqliteService sqliteService)
{
    public string ExecuteJson(string sql, IDictionary<string, object>? args = null)
    {
        var result = sqliteService.Execute(sql, args);
        return JsonSerializer.Serialize(result);
    }

    public object[][] Execute(string sql, IDictionary<string, object>? args = null) => sqliteService.Execute(sql, args);

    public int ExecuteNonQuery(string sql, IDictionary<string, object>? args = null) =>
        sqliteService.ExecuteNonQuery(sql, args);
}