using NLog;
using VRCX.Core.Utils;

namespace VRCX.Core.Platform.Linux.Services;

public sealed class LinuxSteamFolderService
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly string? _steamPath;
    private readonly string? _vrcPrefixPath;

    public LinuxSteamFolderService()
    {
        _steamPath = InitSteamPath();
        _vrcPrefixPath = InitVrcPrefixPath();
    }

    public string? GetSteamPath() => _steamPath;

    public string? GetVrcPrefixPath() => _vrcPrefixPath;

    #region Init Path

    private string? InitSteamPath()
    {
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        // TODO implement XDG Base Directory Specification?
        // https://specifications.freedesktop.org/basedir/latest/
        var steamPath = Path.Join(homeDirectory, ".local/share/Steam");
        if (IsValidSteamPath(steamPath))
        {
            _logger.Info("Host installed Steam detected.");
            return steamPath;
        }

        var flatpakSteamPath = Path.Join(homeDirectory, ".var/app/com.valvesoftware.Steam/.local/share/Steam");
        if (IsValidSteamPath(flatpakSteamPath))
        {
            _logger.Info("Flatpak Steam detected.");
            return flatpakSteamPath;
        }

        var legacySteamPath = Path.Join(homeDirectory, ".steam/steam");
        if (IsValidSteamPath(legacySteamPath))
        {
            _logger.Info("Legacy Steam path detected.");
            return legacySteamPath;
        }

        _logger.Error("No valid Steam library found.");
        return null;
    }

    private string? InitVrcPrefixPath()
    {
        var steamPath = GetSteamPath();
        if (string.IsNullOrEmpty(steamPath))
        {
            return null;
        }

        var libraryFoldersVdfPath = Path.Join(GetSteamPath(), "config/libraryfolders.vdf");
        var vrcLibraryPath = GetLibraryWithAppId(libraryFoldersVdfPath, VRChatUtils.VRChatSteamAppid);
        if (string.IsNullOrEmpty(vrcLibraryPath))
        {
            _logger.Warn(
                "Falling back to default VRChat path as libraryfolders.vdf was not found OR libraryfolders.vdf does not contain VRChat's appid (438100)");
            vrcLibraryPath = _steamPath;
        }

        _logger.Info($"Using steam library path {vrcLibraryPath}");
        return Path.Join(vrcLibraryPath, $"steamapps/compatdata/{VRChatUtils.VRChatSteamAppid}/pfx");
    }

    #endregion

    private static bool IsValidSteamPath(string path)
    {
        return File.Exists(Path.Join(path, "config/libraryfolders.vdf"));
    }

    private static string? GetLibraryWithAppId(string libraryFoldersVdfPath, string appId)
    {
        if (!File.Exists(libraryFoldersVdfPath))
            return null;

        string? libraryPath = null;
        foreach (var line in File.ReadLines(libraryFoldersVdfPath))
        {
            // Assumes line will be \t\t"path"\t\t"pathToLibrary"
            if (line.Contains("\"path\""))
            {
                var parts = line.Split("\t");
                if (parts.Length < 4)
                    continue;

                libraryPath = parts[4].Replace("\"", "");
            }

            if (line.Contains($"\"{appId}\"") && Directory.Exists(libraryPath))
                return libraryPath;
        }

        return null;
    }
}