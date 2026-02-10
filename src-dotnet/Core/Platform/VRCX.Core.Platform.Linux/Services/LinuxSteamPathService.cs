using Serilog;
using VRCX.Core.Utils;

namespace VRCX.Core.Platform.Linux.Services;

public sealed class LinuxSteamPathService
{
    private readonly ILogger _logger = Log.ForContext<LinuxSteamPathService>();

    private readonly string? _steamPath;
    private readonly string? _steamUserdataPath;
    private readonly string? _vrcPrefixPath;

    public LinuxSteamPathService()
    {
        // TODO If better error handling is implemented, consider removing path cache.
        _steamPath = InitSteamPath();
        _steamUserdataPath = InitSteamUserdataPath();
        _vrcPrefixPath = InitVrcPrefixPath();
    }

    public string? GetSteamPath() => _steamPath;

    public string? GetSteamUserdataPath() => _steamUserdataPath;

    public string? GetVrcPrefixPath() => _vrcPrefixPath;

    #region Init Path

    private string? InitSteamPath()
    {
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        // TODO Implement XDG Base Directory Specification?
        // https://specifications.freedesktop.org/basedir/latest/
        var steamPath = Path.Join(homeDirectory, ".local/share/Steam");
        if (IsValidSteamPath(steamPath))
        {
            _logger.Information("Host installed Steam detected");
            return steamPath;
        }

        var flatpakSteamPath = Path.Join(homeDirectory, ".var/app/com.valvesoftware.Steam/.local/share/Steam");
        if (IsValidSteamPath(flatpakSteamPath))
        {
            _logger.Information("Flatpak Steam detected");
            return flatpakSteamPath;
        }

        var legacySteamPath = Path.Join(homeDirectory, ".steam/steam");
        if (IsValidSteamPath(legacySteamPath))
        {
            _logger.Information("Legacy Steam path detected");
            return legacySteamPath;
        }

        // TODO Need better error handling.
        // For now let's assume that the path always exists, and other methods will check if the path exists.
        _logger.Warning("No valid Steam found, fallback to default Steam path: {DefaultSteamPath}", steamPath);
        return steamPath;
    }

    private string? InitSteamUserdataPath()
    {
        // TODO What about flatpak?
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Join(homeDirectory, ".steam/steam/userdata");
    }

    private string? InitVrcPrefixPath()
    {
        var steamPath = GetSteamPath();
        if (string.IsNullOrEmpty(steamPath))
        {
            _logger.Error("No valid Steam found");
            return null;
        }

        var libraryFoldersVdfPath = Path.Join(GetSteamPath(), "config/libraryfolders.vdf");
        var vrcLibraryPath = GetLibraryWithAppId(libraryFoldersVdfPath, VRChatUtils.VRChatSteamAppid);
        if (string.IsNullOrEmpty(vrcLibraryPath))
        {
            _logger.Warning(
                "Falling back to default VRChat path as libraryfolders.vdf was not found OR libraryfolders.vdf does not contain VRChat's appid {VRChatSteamAppId}",
                VRChatUtils.VRChatSteamAppid);
            vrcLibraryPath = _steamPath;
        }

        _logger.Information("Using steam library path {}", vrcLibraryPath);
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