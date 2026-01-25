using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading;

namespace VRCX.Core.Services;

public class SqliteService(AppStorageService storageService) : IDisposable
{
    private readonly ReaderWriterLockSlim _connectionLock = new();
    private SQLiteConnection? _connection;

    private bool _isLoaded;
    private bool _isDisposed;

    public void Init()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        var dataSource = Program.ConfigLocation;
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