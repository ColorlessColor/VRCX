using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using VRCX.Core.Models.GamePlayerPrefs;

namespace VRCX.Core.AppApi;

public partial class AppApiCore
{
    /// <summary>
    /// Retrieves the value of the specified key from the VRChat group in the windows registry.
    /// </summary>
    /// <param name="key">The name of the key to retrieve.</param>
    /// <returns>The value of the specified key, or null if the key does not exist.</returns>
    public override async Task<object?> GetVRChatRegistryKey(string key) =>
        await _gamePlayPrefsService.GetVRChatRegistryKeyAsync(key);

    public override async Task<string?> GetVRChatRegistryKeyString(string key)
    {
        // for electron
        var value = await _gamePlayPrefsService.GetVRChatRegistryKeyAsync(key);
        return value?.ToString();
    }

    #region Set Key

    /// <summary>
    /// Sets the value of the specified key in the VRChat group in the windows registry.
    /// </summary>
    /// <param name="key">The name of the key to set.</param>
    /// <param name="value">The value to set for the specified key.</param>
    /// <param name="typeInt">The RegistryValueKind type.</param>
    /// <returns>True if the key was successfully set, false otherwise.</returns>
    public override async Task<bool> SetVRChatRegistryKey(string key, object value, int typeInt)
    {
        // the value argument can be....
        //  when type is Binary
        //      JSON in string, JsonElement
        //  when type is DWord
        //      REAL DWord (Int32): int, int in string
        //      Double in DWord (Unity shit): double
        // Good luck :)
        try
        {
            switch (typeInt)
            {
                case 4: // RegistryValueKind.DWord
                    if (ParseIntFromRandomObject(value) is { } intValue)
                    {
                        await _gamePlayPrefsService.SetVRChatRegistryKeyDWordAsync(key, intValue);
                        return true;
                    }

                    if (value is double doubleValue)
                    {
                        await _gamePlayPrefsService.SetVRChatRegistryKeyDWordAsync(key, doubleValue);
                        return true;
                    }

                    throw new ArgumentException("Value type " + value.GetType() + " are not support for DWord",
                        nameof(value));
                case 3: // RegistryValueKind.Binary
                    if (value is string str)
                    {
                        await _gamePlayPrefsService.SetVRChatRegistryKeyBinaryAsync(key, str);
                        return true;
                    }

                    if (value is JsonElement { ValueKind: JsonValueKind.String } jsonElement)
                    {
                        var jsonString = jsonElement.GetString();
                        if (jsonString == null)
                        {
                            throw new InvalidOperationException(
                                "GetString() to JsonElement with ValueKind of String return null");
                        }

                        await _gamePlayPrefsService.SetVRChatRegistryKeyBinaryAsync(key, jsonString);
                        return true;
                    }

                    throw new ArgumentException("Value type " + value.GetType() + " are not support for Binary",
                        nameof(value));
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeInt), typeInt,
                        "Unsupported RegistryValueKind type");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "SetVRChatRegistryKey exception for key: {Key} with value: {Value} and typeInt: {TypeInt}", key, value,
                typeInt);
            return false;
        }
    }

    private static int? ParseIntFromRandomObject(object? value)
    {
        // maybe int, int in string
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "Cannot parse int from null value");
        }

        if (value is int i)
            return i;

        if (value is string s)
        {
            if (int.TryParse(s, out var result))
                return result;

            throw new FormatException("String value is not a valid int: " + value);
        }

        return null;
    }

    #endregion

    public override async Task<Dictionary<string, RegistryKeyValue>> GetVRChatRegistry() =>
        await _gamePlayPrefsService.GetVRChatRegistryAsync();

    public override async Task SetVRChatRegistry(string json)
    {
        await _gamePlayPrefsService.EnsureVRChatRegistryFolderCreatedAsync();

        var registryValues = JsonSerializer.Deserialize<Dictionary<string, RegistryKeyValue>>(json,
            new JsonSerializerOptions
            {
                Converters =
                {
                    new RegistryKeyValueJsonConverter()
                },
                RespectNullableAnnotations = true,
                RespectRequiredConstructorParameters = true
            });

        if (registryValues is null)
            throw new ArgumentException("Deserialized registry values is null", nameof(json));

        foreach (var registryKey in registryValues)
        {
            switch (registryKey.Value)
            {
                case RegistryUtf8BinaryValue utf8BinaryValue:
                    _logger.Debug("Setting VRChat Registry Key Binary: {Key} = {Value}", registryKey.Key,
                        utf8BinaryValue.data);
                    await _gamePlayPrefsService.SetVRChatRegistryKeyBinaryAsync(registryKey.Key, utf8BinaryValue.data);

                    break;
                case RegistryDoubleInDWordValue doubleInDWordValue:
                    _logger.Debug("Setting VRChat Registry Key DoubleInDWord: {Key} = {Value}", registryKey.Key,
                        doubleInDWordValue.data);
                    await _gamePlayPrefsService.SetVRChatRegistryKeyDWordAsync(registryKey.Key,
                        doubleInDWordValue.data);

                    break;
                case RegistryDWordValue dWordValue:
                    _logger.Debug("Setting VRChat Registry Key DWord: {Key} = {Value}", registryKey.Key,
                        dWordValue.data);
                    await _gamePlayPrefsService.SetVRChatRegistryKeyDWordAsync(registryKey.Key, dWordValue.data);

                    break;
                default:
                    throw new InvalidOperationException("Unsupported RegistryKeyValue type: " + registryKey.Value.type +
                                                        " " + registryKey.Value.GetType());
            }
        }
    }

    public override async Task<bool> HasVRChatRegistryFolder() =>
        await _gamePlayPrefsService.HasVRChatRegistryFolderAsync();

    public override async Task DeleteVRChatRegistryFolder() =>
        await _gamePlayPrefsService.DeleteVRChatRegistryFolderAsync();

    public override string ReadVrcRegJsonFile(string filepath)
    {
        if (!File.Exists(filepath))
            return string.Empty;

        var json = File.ReadAllText(filepath);
        return json;
    }
}