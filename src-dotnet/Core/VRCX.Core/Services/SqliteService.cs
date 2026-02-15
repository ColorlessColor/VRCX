using System.Data.SQLite;
using System.Text.Json;
using System.Text.Json.Nodes;
using Serilog;
using Serilog.Context;
using VRCX.Core.Utils;

namespace VRCX.Core.Services;

public class SqliteService(AppStorageService storageService) : IDisposable
{
    private readonly ILogger _logger = Log.ForContext<SqliteService>();

    private readonly ReaderWriterLockSlim _connectionLock = new();
    private SQLiteConnection? _connection;

    private bool _isLoaded;
    private bool _isDisposed;

    public void Init()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        var dataSource = AppPathService.ConfigLocation;
        var jsonDataSource = storageService.Get("VRCX_DatabaseLocation");
        if (!string.IsNullOrEmpty(jsonDataSource))
            dataSource = jsonDataSource;

        _connection =
            new SQLiteConnection(
                $"Data Source=\"{dataSource}\";Version=3;PRAGMA locking_mode=NORMAL;PRAGMA busy_timeout=5000;PRAGMA journal_mode=WAL;PRAGMA optimize=0x10002;",
                true);

        _connection.Open();

        _isLoaded = true;
    }

    public void Dispose()
    {
        _isDisposed = true;

        _connectionLock.Dispose();
        _connection?.Dispose();
    }

    public long ExecuteArgsAsJsonNonQuery(string sql, string? argsInJson)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (!_isLoaded)
            throw new InvalidOperationException("Database not initialized.");

        using (LogContext.PushProperty("SqlCommand", sql))
        using (LogContext.PushProperty("ExecuteSqlNonQuery", true))
        {
            _logger.Verbose("Executing SQL non-query {Sql}", sql);

            try
            {
                _connectionLock.EnterReadLock();

                var args = ParseSqliteParametersFromJson(argsInJson);
                using var command = new SQLiteCommand(sql, _connection);

                command.Parameters.AddRange(args);

                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to execute SQL {Sql}", sql);
                throw;
            }
            finally
            {
                _connectionLock.ExitReadLock();
            }
        }
    }

    public string ExecuteArgsAsJson(string sql, string? argsInJson)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (!_isLoaded)
            throw new InvalidOperationException("Database not initialized.");

        using (LogContext.PushProperty("SqlCommand", sql))
        {
            _logger.Verbose("Executing SQL {Sql}", sql);

            try
            {
                _connectionLock.EnterReadLock();

                var args = ParseSqliteParametersFromJson(argsInJson);
                using var command = new SQLiteCommand(sql, _connection);

                command.Parameters.AddRange(args);

                var reader = command.ExecuteReader();
                return SerializeResultToJson(reader);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to execute SQL {Sql}", sql);
                throw;
            }
            finally
            {
                _connectionLock.ExitReadLock();
            }
        }
    }

    private string SerializeResultToJson(SQLiteDataReader reader)
    {
        var jsonRowArray = new JsonArray();

        while (reader.Read())
        {
            var jsonRowColumnsArray = new JsonArray();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var valueAsObject = reader.GetValue(i);
                if (valueAsObject is DBNull)
                {
                    jsonRowColumnsArray.Add(null);
                    continue;
                }

                if (!JsonUtils.TryGetJsonValueFromBaseType(valueAsObject, out var jsonValue))
                    throw new InvalidOperationException("Unsupported data type in SQL result for JSON serialization: " +
                                                        valueAsObject.GetType().FullName);

                jsonRowColumnsArray.Add(jsonValue);
            }

            jsonRowArray.Add(jsonRowColumnsArray);
        }

        return jsonRowArray.ToJsonString();
    }

    private SQLiteParameter[] ParseSqliteParametersFromJson(string? json)
    {
        if (json is null)
            return [];

        var jsonDoc = JsonDocument.Parse(json);
        if (jsonDoc.RootElement.ValueKind == JsonValueKind.Null)
            return [];

        if (jsonDoc.RootElement.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("Expected JSON key-value object for SQL parameters", nameof(json));

        var objectProps = jsonDoc.RootElement.EnumerateObject().ToArray();
        if (objectProps.Any(p => p.Value.ValueKind == JsonValueKind.Object))
            throw new ArgumentException("Expected JSON key-value object for SQL parameters", nameof(json));

        return objectProps
            .Select(p => new SQLiteParameter(p.Name, GetObjectValueFromJsonElement(p.Value)))
            .ToArray();
    }

    private object GetObjectValueFromJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? throw new InvalidOperationException(
                "GetString returned null for a JSON string value, it should NEVER happen."),
            // NOTE: only int32 are support for now
            JsonValueKind.Number => element.TryGetInt64(out var i)
                ? i
                : throw new InvalidOperationException("Only Int64 are supported for SQLite args JSON number values."),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => DBNull.Value,
            _ => throw new ArgumentException($"Unsupported JSON value type: {element.ValueKind}")
        };
    }

    // TODO: Make new api for .net code
    [Obsolete("TODO: Make new api for .net code")]
    public object[][] Execute(string sql, IDictionary<string, object>? args = null)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (!_isLoaded)
            throw new InvalidOperationException("Database not initialized.");

        _connectionLock.EnterReadLock();
        try
        {
            using var command = new SQLiteCommand(sql, _connection);
            if (args != null)
            {
                foreach (var arg in args)
                {
                    command.Parameters.Add(new SQLiteParameter(arg.Key, arg.Value));
                }
            }

            using var reader = command.ExecuteReader();
            var result = new List<object[]>();
            while (reader.Read())
            {
                var values = new object[reader.FieldCount];
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    values[i] = reader.GetValue(i);
                }

                result.Add(values);
            }

            return result.ToArray();
        }
        finally
        {
            _connectionLock.ExitReadLock();
        }
    }

    [Obsolete("TODO: Make new api for .net code")]
    public int ExecuteNonQuery(string sql, IDictionary<string, object>? args = null)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (!_isLoaded)
            throw new InvalidOperationException("Database not initialized.");

        _connectionLock.EnterWriteLock();
        try
        {
            using var command = new SQLiteCommand(sql, _connection);
            if (args != null)
            {
                foreach (var arg in args)
                {
                    command.Parameters.Add(new SQLiteParameter(arg.Key, arg.Value));
                }
            }

            return command.ExecuteNonQuery();
        }
        finally
        {
            _connectionLock.ExitWriteLock();
        }
    }
}