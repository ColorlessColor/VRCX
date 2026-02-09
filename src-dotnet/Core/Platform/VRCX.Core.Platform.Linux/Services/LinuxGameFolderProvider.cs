using NLog;
using VRCX.Core.Services.Platform;
using VRCX.Core.Utils;

namespace VRCX.Core.Platform.Linux.Services;

public sealed class LinuxGameFolderProvider(LinuxSteamFolderService steamFolderService) : IGameFolderProvider
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public string GetVRChatCacheLocation()
    {
        throw new NotImplementedException();
    }

    public string GetVRChatAppDataLocation()
    {
        throw new NotImplementedException();
    }

    public string GetVRChatPhotosLocation()
    {
        throw new NotImplementedException();
    }

    public string GetVRChatCrasphDumpsLocation()
    {
        throw new NotImplementedException();
    }

    public string GetSteamUserdataPath()
    {
        throw new NotImplementedException();
    }
}