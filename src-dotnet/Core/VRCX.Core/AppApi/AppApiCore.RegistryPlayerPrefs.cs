using System.Diagnostics;
using System.Text.Json;
using VRCX.Core.Models.GamePlayerPrefs;
using VRCX.Core.Utils;

namespace VRCX.Core.AppApi;

public partial class AppApiCore
{
    public override async Task<string> GetVRChatRegistryKeyAsJsonString(string key)
    {
        var value = await _gamePlayPrefsService.GetVRChatRegistryKeyAsync(key);
        if (!JsonUtils.TryGetJsonValueFromBaseType(value, out var jsonValue))
        {
            Debug.Fail(
                "GetVRChatRegistryKeyAsJsonString got value that can be directly converted to JsonValue, this should not happen, value type: " +
                value?.GetType());
            throw new Exception(
                "GetVRChatRegistryKeyAsJsonString got value that can be directly converted to JsonValue, this should not happen, value type: " +
                value.GetType());
        }

        return jsonValue?.ToJsonString() ?? "null";
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

                    Debug.Fail("Got unsupported value type " + value.GetType() + "for DWord");
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
                            Debug.Fail("GetString() to JsonElement with ValueKind of String return null");
                            throw new InvalidOperationException(
                                "GetString() to JsonElement with ValueKind of String return null");
                        }

                        await _gamePlayPrefsService.SetVRChatRegistryKeyBinaryAsync(key, jsonString);
                        return true;
                    }

                    Debug.Fail("Got unsupported value type " + value.GetType() + "for Binary");
                    throw new ArgumentException("Value type " + value.GetType() + " are not support for Binary",
                        nameof(value));
                default:
                    Debug.Fail("Got unsupported RegistryValueKind type " + typeInt);
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