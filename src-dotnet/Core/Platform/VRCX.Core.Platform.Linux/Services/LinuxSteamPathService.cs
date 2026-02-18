using System.Text.RegularExpressions;
using Serilog;
using VRCX.Core.Utils;

namespace VRCX.Core.Platform.Linux.Services;

public sealed partial class LinuxSteamPathService
{
    private readonly ILogger _logger = Log.ForContext<LinuxSteamPathService>();

    public string? SteamPath => field ??= TrySteamPath();

    public string? SteamUserdataPath => field ??= TryGetSteamUserdataPath();

    public string? VrcPrefixPath => field ??= TryGetVrcPrefixPath();

    public string? VrcWinePath => field ??= TryGetVrcWinePath();

    private string? TrySteamPath()
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

    private string? TryGetSteamUserdataPath()
    {
        // TODO What about flatpak?
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Join(homeDirectory, ".steam/steam/userdata");
    }

    private string? TryGetVrcPrefixPath()
    {
        var steamPath = SteamPath;
        if (string.IsNullOrEmpty(steamPath))
        {
            _logger.Error("No valid Steam found");
            return null;
        }

        var libraryFoldersVdfPath = Path.Join(SteamPath, "config/libraryfolders.vdf");
        var vrcLibraryPath = GetLibraryWithAppId(libraryFoldersVdfPath, VRChatUtils.VRChatSteamAppid);
        if (string.IsNullOrEmpty(vrcLibraryPath))
        {
            _logger.Warning(
                "Falling back to default VRChat path as libraryfolders.vdf was not found OR libraryfolders.vdf does not contain VRChat's appid {VRChatSteamAppId}",
                VRChatUtils.VRChatSteamAppid);
            vrcLibraryPath = SteamPath;
        }

        _logger.Information("Using steam library path {}", vrcLibraryPath);
        return Path.Join(vrcLibraryPath, $"steamapps/compatdata/{VRChatUtils.VRChatSteamAppid}/pfx");
    }

    private string? TryGetVrcWinePath()
    {
        var compatTool = TryGetSteamVdfCompatTool();
        if (compatTool == null)
        {
            _logger.Error("CompatTool not found");
            return null;
        }

        var steamAppsCommonPath = Path.Join(SteamPath, "steamapps", "common");
        var compatabilityToolsPath = Path.Join(SteamPath, "compatibilitytools.d");
        var protonPath = Path.Join(steamAppsCommonPath, compatTool);
        var compatToolPath = Path.Join(compatabilityToolsPath, compatTool);
        var winePath = "";
        if (Directory.Exists(compatToolPath))
        {
            winePath = Path.Join(compatToolPath, "files", "bin", "wine");
            if (!File.Exists(winePath))
            {
                Console.WriteLine("Wine not found in CompatTool path");
                return null;
            }
        }
        else if (Directory.Exists(protonPath))
        {
            winePath = Path.Join(protonPath, "dist", "bin", "wine");
            if (!File.Exists(winePath))
            {
                _logger.Error("Wine not found in Proton path");
                return null;
            }
        }
        else if (Directory.Exists(compatabilityToolsPath))
        {
            var dirs = Directory.GetDirectories(compatabilityToolsPath);
            foreach (var dir in dirs)
            {
                if (dir.Contains(compatTool))
                {
                    winePath = Path.Join(dir, "files", "bin", "wine");
                    if (File.Exists(winePath))
                    {
                        break;
                    }
                }
            }

            if (!File.Exists(winePath))
            {
                Console.WriteLine("Wine not found in CompatTool path");
                return null;
            }
        }

        if (winePath == "")
        {
            _logger.Error("CompatTool and Proton not found");
            return null;
        }

        return winePath;
    }


    private string? TryGetSteamVdfCompatTool()
    {
        var configVdfPath = Path.Join(SteamPath, "config", "config.vdf");
        if (!File.Exists(configVdfPath))
        {
            _logger.Error("config.vdf not found");
            return null;
        }

        var vdfContent = File.ReadAllText(configVdfPath);
        var compatToolMapping = ExtractCompatToolMapping(vdfContent);

        if (compatToolMapping.TryGetValue("438100", out var name))
        {
            return name;
        }

        return null;
    }

    private Dictionary<string, string> ExtractCompatToolMapping(string vdfContent)
    {
        var compatToolMapping = new Dictionary<string, string>();
        const string sectionHeader = "\"CompatToolMapping\"";
        var sectionStart = vdfContent.IndexOf(sectionHeader, StringComparison.Ordinal);

        if (sectionStart == -1)
        {
            _logger.Error("CompatToolMapping not found");
            return compatToolMapping;
        }

        var blockStart = vdfContent.IndexOf('{', sectionStart) + 1;
        var blockEnd = FindMatchingBracket(vdfContent, blockStart - 1);

        if (blockStart == -1 || blockEnd == -1)
        {
            _logger.Error("CompatToolMapping block not found");
            return compatToolMapping;
        }

        var blockContent = vdfContent.Substring(blockStart, blockEnd - blockStart);

        var matches = KeyValuePattern().Matches(blockContent);
        foreach (Match match in matches)
        {
            var key = match.Groups[1].Value;
            var name = match.Groups[2].Value;

            if (key != "0")
            {
                compatToolMapping[key] = name;
            }
        }

        return compatToolMapping;
    }

    #region Utility

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

    private static int FindMatchingBracket(string content, int openBracketIndex)
    {
        var depth = 0;
        for (var i = openBracketIndex; i < content.Length; i++)
        {
            if (content[i] == '{')
                depth++;
            else if (content[i] == '}')
            {
                depth--;
                if (depth == 0)
                    return i;
            }
        }

        return -1;
    }

    [GeneratedRegex("\"(\\d+)\"\\s*\\{[^}]*\"name\"\\s*\"([^\"]+)\"", RegexOptions.Multiline)]
    private static partial Regex KeyValuePattern();

    #endregion
}