using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Win32;
using NLog;
using VRCX.Core.Models.GamePlayerPrefs;
using VRCX.Core.Services.Platform;
using VRCX.Core.Utils;
using VRCX.Core.Windows.Interop;

namespace VRCX.Core.Windows.Services;

public sealed class WindowsPlayerPrefsService : IGamePlayPrefsService
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private const string VRChatRegistryPath = @"SOFTWARE\VRChat\VRChat";

    public async ValueTask EnsureVRChatRegistryFolderCreatedAsync()
    {
        await Task.Run(async () =>
        {
            if (await HasVRChatRegistryFolderAsync())
                return;

            Registry.CurrentUser.CreateSubKey(VRChatRegistryPath);
        });
    }

    public async ValueTask<bool> HasVRChatRegistryFolderAsync()
    {
        return await Task.Run(() => TryGetVRChatRegistryKey() is not null);
    }

    #region Get Key

    public async ValueTask<object?> GetVRChatRegistryKeyAsync(string key)
    {
        return await Task.Run(() =>
        {
            try
            {
                return GetVRChatRegistryKeyCore(key);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get VRChat Registry key: {Key}", key);
                return null;
            }
        });
    }

    private object? GetVRChatRegistryKeyCore(string key)
    {
        var keyName = PlayerPrefsUtils.AddHashToKeyName(key);

        using var regKey = TryGetVRChatRegistryKey();
        if (regKey is null)
        {
            _logger.Warn("VRChat Registry key not found: {RawKey} => {HashKey}", key, keyName);
            return null;
        }

        var data = regKey.GetValue(keyName);
        if (data == null)
        {
            _logger.Warn("Get VRChat Registry key value retruned null: {RawKey} => {HashKey}", key, keyName);
            return null;
        }

        var type = regKey.GetValueKind(keyName);
        switch (type)
        {
            case RegistryValueKind.Binary:
                if (data is not byte[] dataInBytes)
                {
                    _logger.Error(
                        "Get VRChat Registry binary key value is not byte[] (IT SHOULDN'T HAPPEAN): {RawKey} => {HashKey} ({Type})",
                        key, keyName, data.GetType());
                    return null;
                }

                return Encoding.UTF8.GetString(dataInBytes).Replace("\0", "");

            case RegistryValueKind.DWord:
                if (data is long dataInLong)
                {
                    // Unity stores double as DWord in Registry
                    var bytes = BitConverter.GetBytes(dataInLong);
                    return BitConverter.ToDouble(bytes, 0);
                }

                if (data is int dataInInt)
                {
                    return dataInInt;
                }

                _logger.Error(
                    "Get VRChat Registry DWord key value is not long neither int (IT SHOULDN'T HAPPEAN): {RawKey} => {HashKey} ({Type})",
                    key, keyName, data.GetType());
                return null;
        }

        _logger.Warn("Unsupported VRChat Registry value type: {RawKey} => {HashKey} ({Type})", key, keyName, type);
        return null;
    }

    #endregion

    #region Serialize Registry

    public async ValueTask<Dictionary<string, RegistryKeyValue>> GetVRChatRegistryAsync()
    {
        return await Task.Run(GetVRChatRegistryCore);
    }

    private Dictionary<string, RegistryKeyValue> GetVRChatRegistryCore()
    {
        var result = new Dictionary<string, RegistryKeyValue>();

        using var regKey = TryGetVRChatRegistryKey();
        if (regKey is null)
            ThrowVRChatRegistryFolderNotFound();

        var keys = regKey.GetValueNames();
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
                    if (data is not byte[] bytes)
                    {
                        throw new InvalidOperationException("Registry binary data is not byte[]: " +
                                                            data.GetType());
                    }

                    result.Add(keyName, new RegistryUtf8BinaryValue(Encoding.UTF8.GetString(bytes)));
                    break;
                case RegistryValueKind.DWord:
                    switch (data)
                    {
                        case long dataInLong:
                            var dwordBytes = BitConverter.GetBytes(dataInLong);
                            var doubleValue = BitConverter.ToDouble(dwordBytes, 0);
                            result.Add(keyName, new RegistryDoubleInDWordValue(doubleValue));
                            break;
                        case int dataInInt:
                            result.Add(keyName, new RegistryDWordValue(dataInInt));
                            break;
                        default:
                            throw new InvalidOperationException("Registry DWord data is not long neither int: " +
                                                                data.GetType());
                    }

                    break;
                default:
                    throw new InvalidOperationException("Unsupported Registry value type: " + type);
            }
        }

        return result;
    }

    #endregion

    #region Set Key

    public async ValueTask SetVRChatRegistryKeyDWordAsync(string key, double value)
    {
        await Task.Run(() => { SetVRChatRegistryKeyDWordCore(key, value); });
    }

    public async ValueTask SetVRChatRegistryKeyDWordAsync(string key, int value)
    {
        await Task.Run(() => { SetVRChatRegistryKeyDWordCore(key, value); });
    }

    public async ValueTask SetVRChatRegistryKeyBinaryAsync(string key, string value)
    {
        await Task.Run(() => { SetVRChatRegistryKeyBinaryCore(key, value); });
    }

    private void SetVRChatRegistryKeyDWordCore(string key, double value)
    {
        var keyName = PlayerPrefsUtils.AddHashToKeyName(key);
        using var regKey = TryGetVRChatRegistryKey(true);
        if (regKey is null)
            ThrowVRChatRegistryFolderNotFound();

        var dataInBytes = BitConverter.GetBytes(value);
        Advapi32Interop.RegSetValueEx(
            regKey.Handle, keyName, 0, (int)RegistryValueKind.DWord, dataInBytes, dataInBytes.Length);
    }

    private void SetVRChatRegistryKeyDWordCore(string key, int value)
    {
        var keyName = PlayerPrefsUtils.AddHashToKeyName(key);
        using var regKey = TryGetVRChatRegistryKey(true);
        if (regKey is null)
            ThrowVRChatRegistryFolderNotFound();

        var dataInBytes = BitConverter.GetBytes(value);
        Advapi32Interop.RegSetValueEx(
            regKey.Handle, keyName, 0, (int)RegistryValueKind.DWord, dataInBytes, dataInBytes.Length);
    }

    private void SetVRChatRegistryKeyBinaryCore(string key, string value)
    {
        var keyName = PlayerPrefsUtils.AddHashToKeyName(key);
        using var regKey = TryGetVRChatRegistryKey(true);
        if (regKey is null)
            ThrowVRChatRegistryFolderNotFound();

        var dataInBytes = Encoding.UTF8.GetBytes(value);
        regKey.SetValue(keyName, dataInBytes, RegistryValueKind.Binary);
    }

    #endregion

    public async ValueTask DeleteVRChatRegistryFolderAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                using var regKey = TryGetVRChatRegistryKey(true);
                if (regKey == null)
                {
                    _logger.Warn("VRChat Registry key not found for deletion: {FolderPath}", VRChatRegistryPath);
                    return;
                }

                Registry.CurrentUser.DeleteSubKeyTree(VRChatRegistryPath);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to delete VRChat Registry folder: {FolderPath}", VRChatRegistryPath);
            }
        });
    }

    private RegistryKey? TryGetVRChatRegistryKey(bool writable = false)
    {
        try
        {
            return Registry.CurrentUser.OpenSubKey(VRChatRegistryPath, writable);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to open VRChat Registry key: {Path}", VRChatRegistryPath);
            return null;
        }
    }

    [DoesNotReturn]
    private void ThrowVRChatRegistryFolderNotFound()
    {
        throw new InvalidOperationException("VRChat Registry folder not found: " + VRChatRegistryPath);
    }
}