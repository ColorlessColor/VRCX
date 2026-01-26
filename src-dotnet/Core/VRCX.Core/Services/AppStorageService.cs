using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using VRCX.Core.Extensions;

namespace VRCX.Core.Services;

public sealed class AppStorageService
{
    private ConcurrentDictionary<string, string> _storage = new();
    private readonly string _jsonPath = Path.Join(AppPathService.AppDataDirectory, "VRCX.json");

    private readonly TimeSpan _saveDebounce = TimeSpan.FromMilliseconds(500);
    private readonly Timer _saveTimer;
    private readonly Lock _saveLock = new Lock();

    public AppStorageService()
    {
        _saveTimer = new Timer(_ => Save(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public void Load()
    {
        var tmp = new Dictionary<string, string>();
        JsonFileSerializer.Deserialize(_jsonPath, ref tmp);
        _storage = new ConcurrentDictionary<string, string>(tmp);
    }

    public void Save()
    {
        lock (_saveLock)
        {
            var snapshot = new Dictionary<string, string>(_storage);
            JsonFileSerializer.Serialize(_jsonPath, snapshot);
        }
    }

    public void Clear()
    {
        if (!_storage.IsEmpty)
        {
            _storage.Clear();
            ScheduleSave();
        }
    }

    public bool Remove(string key)
    {
        var result = _storage.TryRemove(key, out _);
        if (result)
            ScheduleSave();
        return result;
    }

    public string Get(string key)
    {
        return _storage.TryGetValue(key, out var value) ? value : string.Empty;
    }

    public void Set(string key, string value)
    {
        _storage[key] = value;
        ScheduleSave();
    }

    public string GetAll()
    {
        return JsonSerializer.Serialize(new Dictionary<string, string>(_storage));
    }

    private void ScheduleSave()
    {
        _saveTimer.Change(_saveDebounce, Timeout.InfiniteTimeSpan);
    }
}