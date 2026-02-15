using VRCX.Core.Services;

namespace VRCX.Core.WebViewInterop;

public class SQLite(SqliteService sqliteService)
{
    public string ExecuteArgsAsJson(string sql, string? argsInJson) =>
        sqliteService.ExecuteArgsAsJson(sql, argsInJson);

    public long ExecuteArgsAsJsonNonQuery(string sql, string? args) =>
        sqliteService.ExecuteArgsAsJsonNonQuery(sql, args);

    [Obsolete("Use ExecuteArgsAsJson instead")]
    public string ExecuteJson(string sql, IDictionary<string, object>? args = null) =>
        throw new NotSupportedException("Use ExecuteArgsAsJson instead");

    [Obsolete("Use ExecuteArgsAsJson instead")]
    public object[][] Execute(string sql, IDictionary<string, object>? args = null) =>
        throw new NotSupportedException("Use ExecuteArgsAsJson instead");

    [Obsolete("Use ExecuteArgsAsJsonNonQuery instead")]
    public int ExecuteNonQuery(string sql, IDictionary<string, object>? args = null) =>
        throw new NotSupportedException("Use ExecuteArgsAsJsonNonQuery instead");
}