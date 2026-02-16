using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;
using VRCX.Core.Shared;

namespace VRCX.Core.Services;

public sealed class AppStorageService
{
    private readonly ILogger _logger = Log.ForContext<AppStorageService>();

    private Dictionary<string, string> _storage = new();
    private readonly Lock _storageLock = new();
    private readonly Lock _saveLock = new();

    private readonly string _jsonPath = Path.Join(AppPathService.AppDataDirectory, "VRCX.json");

    public void Load()
    {
        try
        {
            if (!File.Exists(_jsonPath))
            {
                _logger.Information("No existing storage file found at {Path}. Starting with empty storage", _jsonPath);
                return;
            }

            var jsonContent = File.ReadAllText(_jsonPath);
            var storage = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent,
                AppStorageJsonContext.Default.DictionaryStringString);

            if (storage == null)
            {
                _logger.Warning("Storage file at {Path} is null json. Starting with empty storage", _jsonPath);
                return;
            }

            lock (_storageLock)
            {
                _storage = storage;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load storage from {Path}. Starting with empty storage", _jsonPath);
        }
    }

    public void Save()
    {
        lock (_saveLock)
        {
            var snapshot = GetSnapshot();
            try
            {
                var storageJson =
                    JsonSerializer.Serialize(snapshot, AppStorageJsonContext.Default.DictionaryStringString);

                File.WriteAllText(_jsonPath, storageJson);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save storage to {Path}", _jsonPath);
            }
        }
    }

    public void Clear()
    {
        lock (_storageLock)
        {
            _storage.Clear();
        }

        Save();
    }

    public bool Remove(string key)
    {
        bool result;
        lock (_storageLock)
        {
            result = _storage.Remove(key);
        }

        Save();
        return result;
    }

    public string Get(string key)
    {
        lock (_storageLock)
        {
            return _storage.TryGetValue(key, out var value) ? value : string.Empty;
        }
    }

    public void Set(string key, string value)
    {
        lock (_storageLock)
        {
            _storage[key] = value;
        }

        Save();
    }

    public string GetAll()
    {
        return JsonSerializer.Serialize(GetSnapshot(), AppStorageJsonContext.Default.DictionaryStringString);
    }

    private Dictionary<string, string> GetSnapshot()
    {
        lock (_storageLock)
        {
            return new Dictionary<string, string>(_storage);
        }
    }
}

[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal sealed partial class AppStorageJsonContext : JsonSerializerContext;