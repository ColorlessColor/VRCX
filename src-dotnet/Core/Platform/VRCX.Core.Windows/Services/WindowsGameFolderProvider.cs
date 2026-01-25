using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Windows.Services;

public class WindowsGameFolderProvider : IGameFolderProvider
{
    public string GetVRChatCacheLocation()
    {
        var defaultPath = Path.Join(GetVRChatAppDataLocation(), "Cache-WindowsPlayer");
        try
        {
            var json = ReadConfigFile();
            if (string.IsNullOrEmpty(json))
                return defaultPath;

            var obj = JsonConvert.DeserializeObject<JObject>(json);
            if (obj["cache_directory"] == null)
                return defaultPath;

            var cacheDir = (string)obj["cache_directory"];
            if (string.IsNullOrEmpty(cacheDir))
                return defaultPath;

            var cachePath = Path.Join(cacheDir, "Cache-WindowsPlayer");
            if (!Directory.Exists(cacheDir))
                return defaultPath;

            return cachePath;
        }
        catch (Exception e)
        {
            // logger.Error($"Error reading VRChat config file for cache location: {e}");
        }

        return defaultPath;
    }

    public string GetVRChatAppDataLocation()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"Low\VRChat\VRChat";
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