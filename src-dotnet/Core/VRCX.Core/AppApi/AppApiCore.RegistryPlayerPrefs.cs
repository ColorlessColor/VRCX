using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
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

    public override async Task<bool> SetVRChatRegistryKeyFromJsonString(
        string key, string valueAsJsonString, int typeInt
    )
    {
        try
        {
            var jsonNode = JsonNode.Parse(valueAsJsonString);
            if (jsonNode is null)
                throw new ArgumentNullException(nameof(valueAsJsonString), "Cannot parse null JSON string to JsonNode");

            switch (typeInt)
            {
                case 4: // RegistryValueKind.DWord
                    if (jsonNode.GetValueKind() != JsonValueKind.Number)
                        throw new ArgumentException("Value is not a Number");

                    if (jsonNode.AsValue().TryGetValue<int>(out var intValue))
                    {
                        await _gamePlayPrefsService.SetVRChatRegistryKeyDWordAsync(key, intValue);
                        return true;
                    }

                    if (jsonNode.AsValue().TryGetValue<double>(out var doubleValue))
                    {
                        await _gamePlayPrefsService.SetVRChatRegistryKeyDWordAsync(key, doubleValue);
                        return true;
                    }

                    Debug.Fail("Got unsupported json value " + jsonNode + "for DWord");
                    throw new ArgumentException("JsonValue " + jsonNode + " are not support for DWord",
                        nameof(valueAsJsonString));
                case 3: // RegistryValueKind.Binary
                    if (jsonNode.GetValueKind() != JsonValueKind.String)
                        throw new ArgumentException("Value is not a String", nameof(valueAsJsonString));

                    if (jsonNode.AsValue().TryGetValue<string>(out var stringValue))
                    {
                        await _gamePlayPrefsService.SetVRChatRegistryKeyBinaryAsync(key, stringValue);
                        return true;
                    }

                    Debug.Fail("TryGetValue<string>() to JsonValue with ValueKind of String fail");
                    throw new InvalidOperationException(
                        "TryGetValue<string>() to JsonValue with ValueKind of String fail");
                default:
                    Debug.Fail("Got unsupported RegistryValueKind type " + typeInt);
                    throw new ArgumentOutOfRangeException(nameof(typeInt), typeInt,
                        "Unsupported RegistryValueKind type");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "SetVRChatRegistryKey exception for key: {Key} with value: {Value} and typeInt: {TypeInt}",
                key,
                valueAsJsonString,
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

        var registryValues = JsonSerializer.Deserialize<Dictionary<string, RegistryKeyValue>>(
            json,
            RegistryKeyValueJsonContext.Default.DictionaryStringRegistryKeyValue
        );

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