using VRCX.Core.Models.GamePlayerPrefs;

namespace VRCX.Core.Services.Platform;

public interface IGamePlayPrefsService
{
    ValueTask EnsureVRChatRegistryFolderCreatedAsync();
    ValueTask<bool> HasVRChatRegistryFolderAsync();

    ValueTask<object?> GetVRChatRegistryKeyAsync(string key);
    ValueTask<Dictionary<string, RegistryKeyValue>> GetVRChatRegistryAsync();

    ValueTask SetVRChatRegistryKeyDWordAsync(string key, double value);
    ValueTask SetVRChatRegistryKeyDWordAsync(string key, int value);
    ValueTask SetVRChatRegistryKeyBinaryAsync(string key, string value);

    ValueTask SetVRChatRegistryFromJson(string json);

    ValueTask DeleteVRChatRegistryFolderAsync();
}