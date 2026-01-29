using System.Collections.Generic;
using System.Threading.Tasks;

namespace VRCX.Core.Services.Platform;

public interface IGamePlayPrefsService
{
    ValueTask<bool> HasVRChatRegistryFolderAsync();

    ValueTask<object?> GetVRChatRegistryKeyAsync(string key);
    ValueTask<Dictionary<string, Dictionary<string, object>>> GetVRChatRegistry();

    ValueTask SetVRChatRegistryKeyDWordAsync(string key, double value);
    ValueTask SetVRChatRegistryKeyDWordAsync(string key, int value);
    ValueTask SetVRChatRegistryKeyBinaryAsync(string key, string value);

    ValueTask SetVRChatRegistryFromJson(string json);

    ValueTask DeleteVRChatRegistryFolderAsync();
}