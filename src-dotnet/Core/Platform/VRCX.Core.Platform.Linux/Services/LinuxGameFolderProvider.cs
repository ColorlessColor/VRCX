using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using VRCX.Core.Services.Platform;
using VRCX.Core.Utils;

namespace VRCX.Core.Platform.Linux.Services;

public sealed class LinuxGameFolderProvider(LinuxSteamPathService steamPathService) : IGameFolderProvider
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public string GetVRChatCacheLocation()
    {
        var defaultPath = Path.Join(GetVRChatAppDataLocation(), "Cache-WindowsPlayer");
        try
        {
            var json = ReadConfigFile();
            if (string.IsNullOrEmpty(json))
            {
                _logger.Debug("VRChat config file is empty or missing, using default photos path");
                return defaultPath;
            }

            var jsonObject = JObject.Parse(json);
            if (jsonObject["cache_directory"] is not { } cacheDirectoryKey)
            {
                _logger.Debug("cache_directory key not found in VRChat config file, using default path");
                return defaultPath;
            }

            if (cacheDirectoryKey.Type != JTokenType.String)
                throw new InvalidOperationException("cache_directory key is not a string in VRChat config file");

            var cacheDir = cacheDirectoryKey.ToString();
            if (string.IsNullOrWhiteSpace(cacheDir))
                throw new InvalidOperationException("cache_directory value is empty in VRChat config file");

            return Path.Join(cacheDir, "Cache-WindowsPlayer");
        }
        catch (Exception e)
        {
            _logger.Warn(e, "Error reading VRChat config file for cache location, fall back to default path");
        }

        return defaultPath;
    }

    public string GetVRChatAppDataLocation()
    {
        var vrcPrefixPath = steamPathService.GetVrcPrefixPath();
        if (string.IsNullOrEmpty(vrcPrefixPath))
        {
            throw new InvalidOperationException("Failed to get VRChat AppData folder path: VRChat prefix not found");
        }

        return Path.Join(vrcPrefixPath, "drive_c/users/steamuser/AppData/LocalLow/VRChat/VRChat");
    }

    public string GetVRChatPhotosLocation()
    {
        var vrcPrefixPath = steamPathService.GetVrcPrefixPath();
        if (string.IsNullOrEmpty(vrcPrefixPath))
        {
            throw new InvalidOperationException("Failed to get VRChat AppData folder path: VRChat prefix not found");
        }

        var defaultPath = Path.Join(vrcPrefixPath, "drive_c/users/steamuser/Pictures/VRChat");

        try
        {
            var json = ReadConfigFile();
            if (string.IsNullOrEmpty(json))
            {
                _logger.Debug("VRChat config file is empty or missing, using default photos path");
                return defaultPath;
            }

            var obj = JObject.Parse(json);
            if (obj["picture_output_folder"] is not { } pictureOutputFolderKey)
            {
                _logger.Debug("picture_output_folder key not found in VRChat config file, using default path");
                return defaultPath;
            }

            if (pictureOutputFolderKey.Type != JTokenType.String)
                throw new InvalidOperationException("picture_output_folder key is not a string in VRChat config file");

            return pictureOutputFolderKey.ToString();
        }
        catch (Exception e)
        {
            _logger.Warn(e, "Error reading VRChat config file for photos location, fall back to default path.");
        }

        return defaultPath;
    }

    public string GetVRChatCrashDumpsLocation()
    {
        var vrcPrefixPath = steamPathService.GetVrcPrefixPath();
        if (string.IsNullOrEmpty(vrcPrefixPath))
        {
            throw new InvalidOperationException(
                "Failed to get VRChat crash dumps folder path: VRChat prefix not found");
        }

        return Path.Join(vrcPrefixPath, "drive_c/users/steamuser/AppData/Local/Temp/VRChat/VRChat/Crashes");
    }

    public string GetSteamUserdataPath()
    {
        // TODO: Fix Steam userdata path, for now just get the first folder
        var steamUserDataPath = steamPathService.GetSteamUserdataPath();
        if (Directory.Exists(steamUserDataPath))
        {
            var steamUserDirs = Directory.GetDirectories(steamUserDataPath);
            if (steamUserDirs.Length > 0)
            {
                return steamUserDirs[0];
            }
        }

        return string.Empty;
    }

    private string ReadConfigFile()
    {
        var path = GetVRChatAppDataLocation();
        var configFile = Path.Join(path, "config.json");

        if (!Directory.Exists(path) || !File.Exists(configFile))
        {
            return string.Empty;
        }

        var json = File.ReadAllText(configFile);
        return json;
    }
}