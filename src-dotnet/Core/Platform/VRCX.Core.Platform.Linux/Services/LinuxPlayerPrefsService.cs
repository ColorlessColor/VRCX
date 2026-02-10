using Serilog;
using VRCX.Core.Models.GamePlayerPrefs;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Platform.Linux.Services;

public sealed class LinuxPlayerPrefsService : IGamePlayPrefsService
{
    private readonly ILogger _logger = Log.ForContext<LinuxPlayerPrefsService>();

    private const string VRChatRegistryPath = @"SOFTWARE\VRChat\VRChat";

    public ValueTask EnsureVRChatRegistryFolderCreatedAsync()
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> HasVRChatRegistryFolderAsync()
    {
        throw new NotImplementedException();
    }

    public ValueTask<object?> GetVRChatRegistryKeyAsync(string key)
    {
        throw new NotImplementedException();
    }

    public ValueTask<Dictionary<string, RegistryKeyValue>> GetVRChatRegistryAsync()
    {
        throw new NotImplementedException();
    }

    public ValueTask SetVRChatRegistryKeyDWordAsync(string key, double value)
    {
        throw new NotImplementedException();
    }

    public ValueTask SetVRChatRegistryKeyDWordAsync(string key, int value)
    {
        throw new NotImplementedException();
    }

    public ValueTask SetVRChatRegistryKeyBinaryAsync(string key, string value)
    {
        throw new NotImplementedException();
    }

    public ValueTask DeleteVRChatRegistryFolderAsync()
    {
        throw new NotImplementedException();
    }
}