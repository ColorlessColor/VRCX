using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Win32;
using NLog;
using VRCX.Core.Models.GamePlayerPrefs;
using VRCX.Core.Services.Platform;
using VRCX.Core.Utils;

namespace VRCX.Core.Platform.Linux.Services;

public sealed class LinuxPlayerPrefsService : IGamePlayPrefsService
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

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