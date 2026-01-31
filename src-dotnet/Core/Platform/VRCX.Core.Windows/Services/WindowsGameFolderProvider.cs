using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using NLog;
using VRCX.Core.Services.Platform;
using VRCX.Core.Windows.Interop;

namespace VRCX.Core.Windows.Services;

public class WindowsGameFolderProvider : IGameFolderProvider
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
            _logger.Warn(e, "Error reading VRChat config file for cache location, fall back to default path.");
        }

        return defaultPath;
    }

    public string GetVRChatAppDataLocation()
    {
        var resultCode = Shell32Interop.SHGetKnownFolderPath(Shell32Interop.FolderIdLocalAppDataLow,
            (uint)Environment.SpecialFolderOption.None, IntPtr.Zero, out var path);
        if (resultCode == 0)
            return Path.Combine(path, "VRChat", "VRChat");

        throw new InvalidOperationException("Failed to get LocalAppDataLow folder path via SHGetKnownFolderPath.");
    }

    public string GetVRChatPhotosLocation()
    {
        var defaultPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "VRChat");
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

    public string GetVRChatCrasphDumpsLocation()
    {
        return Path.Join(Path.GetTempPath(), "VRChat", "VRChat", "Crashes");
    }

    public string GetSteamUserdataPath()
    {
        var steamUserdataPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            @"Steam\userdata");

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam");
            if (key is null)
                throw new InvalidOperationException(
                    @"Steam registry key not found (HKLM\SOFTWARE\WOW6432Node\Valve\Steam)");

            const string keyName = "InstallPath";
            if (key.GetValueKind(keyName) != RegistryValueKind.String)
                throw new InvalidOperationException("Steam InstallPath registry value is not a string");

            if (key.GetValue(keyName) is not { } keyValue)
                throw new InvalidOperationException("Get Steam InstallPath registry value retrun null");

            steamUserdataPath = Path.Join(keyValue.ToString(), @"userdata");
        }
        catch (Exception e)
        {
            _logger.Warn(e, "Failed to get Steam userdata path from registry, falling back to default path.");
        }

        return steamUserdataPath;
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