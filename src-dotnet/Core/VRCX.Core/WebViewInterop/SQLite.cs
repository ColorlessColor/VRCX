using VRCX.Core.Services;

namespace VRCX.Core.WebViewInterop;

public class SQLite(SqliteService sqliteService)
{
    public string ExecuteArgsAsJson(string sql, string? argsInJson) =>
        sqliteService.ExecuteArgsAsJson(sql, argsInJson);

    public long ExecuteArgsAsJsonNonQuery(string sql, string? args) =>
        sqliteService.ExecuteArgsAsJsonNonQuery(sql, args);
}