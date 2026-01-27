namespace VRCX.Core.Services.Platform;

public interface IGameFolderProvider
{
    string GetVRChatCacheLocation();
    string GetVRChatAppDataLocation();
    string GetVRChatPhotosLocation();
    string GetVRChatCrasphDumpsLocation();
    string GetSteamUserdataPath();
}