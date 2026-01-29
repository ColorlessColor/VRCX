using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace VRCX.Core.AppApi;

public partial class AppApiCore
{
    [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern uint RegSetValueEx(
        UIntPtr hKey,
        [MarshalAs(UnmanagedType.LPStr)] string lpValueName,
        int Reserved,
        RegistryValueKind dwType,
        byte[] lpData,
        int cbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
    private static extern int RegOpenKeyEx(
        UIntPtr hKey,
        string subKey,
        int ulOptions,
        int samDesired,
        out UIntPtr hkResult);

    [DllImport("advapi32.dll")]
    private static extern int RegCloseKey(UIntPtr hKey);

    private string AddHashToKeyName(string key)
    {
        // https://discussions.unity.com/t/playerprefs-changing-the-name-of-keys/30332/4
        // VRC_GROUP_ORDER_usr_032383a7-748c-4fb2-94e4-bcb928e5de6b_h2810492971
        uint hash = 5381;
        foreach (var c in key)
            hash = (hash * 33) ^ c;
        return key + "_h" + hash;
    }

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

    /// <summary>
    /// Sets the value of the specified key in the VRChat group in the windows registry.
    /// </summary>
    /// <param name="key">The name of the key to set.</param>
    /// <param name="value">The value to set for the specified key.</param>
    [Obsolete("Use SetVRChatRegistryKey with appropriate typeInt instead")]
    public override void SetVRChatRegistryKey(string key, byte[] value)
    {
        var keyName = AddHashToKeyName(key);
        var hKey = (UIntPtr)0x80000001; // HKEY_LOCAL_MACHINE
        const int keyWrite = 0x20006;
        const string keyFolder = @"SOFTWARE\VRChat\VRChat";
        var openKeyResult = RegOpenKeyEx(hKey, keyFolder, 0, keyWrite, out var folderPointer);
        if (openKeyResult != 0)
            throw new Exception("Error opening registry key. Error code: " + openKeyResult);

        var setKeyResult = RegSetValueEx(folderPointer, keyName, 0, RegistryValueKind.DWord, value, value.Length);
        if (setKeyResult != 0)
            throw new Exception("Error setting registry value. Error code: " + setKeyResult);

        RegCloseKey(hKey);
    }

    public override Dictionary<string, Dictionary<string, object>> GetVRChatRegistry()
    {
        var output = new Dictionary<string, Dictionary<string, object>>();
        using var regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\VRChat\VRChat");
        if (regKey == null)
            throw new Exception("Failed to get VRC registry data");

        var keys = regKey.GetValueNames();

        Span<long> spanLong = stackalloc long[1];
        Span<double> doubleSpan = MemoryMarshal.Cast<long, double>(spanLong);

        foreach (var key in keys)
        {
            var data = regKey.GetValue(key);
            var index = key.LastIndexOf("_h", StringComparison.Ordinal);
            if (index <= 0)
                continue;

            var keyName = key.Substring(0, index);
            if (data == null)
                continue;

            var type = regKey.GetValueKind(key);
            switch (type)
            {
                case RegistryValueKind.Binary:
                    var binDict = new Dictionary<string, object>
                    {
                        { "data", Encoding.UTF8.GetString((byte[])data) },
                        { "type", type }
                    };
                    output.Add(keyName, binDict);
                    break;

                case RegistryValueKind.DWord:
                    if (data.GetType() != typeof(long))
                    {
                        var dwordDict = new Dictionary<string, object>
                        {
                            { "data", data },
                            { "type", type }
                        };
                        output.Add(keyName, dwordDict);
                        break;
                    }

                    spanLong[0] = (long)data;
                    var doubleValue = doubleSpan[0];
                    var floatDict = new Dictionary<string, object>
                    {
                        { "data", doubleValue },
                        { "type", 100 } // it's special
                    };
                    output.Add(keyName, floatDict);
                    break;

                default:
                    Debug.WriteLine($"Unknown registry value kind: {type}");
                    break;
            }
        }

        return output;
    }

    public override async Task SetVRChatRegistry(string json)
    {
        await CreateVRChatRegistryFolder();
        var dict = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);
        foreach (var item in dict)
        {
            var data = (JsonElement)item.Value["data"];
            if (!int.TryParse(item.Value["type"].ToString(), out var type))
                throw new Exception("Unknown type: " + item.Value["type"]);

            if (data.ValueKind == JsonValueKind.Number)
            {
                if (type == 100)
                {
                    // fun handling of double to long to byte array
                    var doubleValue = data.Deserialize<double>();
                    await SetVRChatRegistryKey(item.Key, doubleValue, 4);
                    continue;
                }

                if (int.TryParse(data.ToString(), out var intValue))
                {
                    await SetVRChatRegistryKey(item.Key, intValue, type);
                    continue;
                }

                throw new Exception("Unknown number type: " + item.Key);
            }

            await SetVRChatRegistryKey(item.Key, data, type);
        }
    }

    public override async Task<bool> HasVRChatRegistryFolder() =>
        await _gamePlayPrefsService.HasVRChatRegistryFolderAsync();

    private async Task CreateVRChatRegistryFolder()
    {
        if (await HasVRChatRegistryFolder())
            return;

        using var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\VRChat\VRChat");
        if (key == null)
            throw new Exception("Error creating registry key.");
    }

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