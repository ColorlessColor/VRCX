namespace VRCX.Core.Platform.Linux.Utils;

public static class SteamPathUtils
{
    public enum SteamPathType
    {
        HostInstalledSteam,
        FlatpakSteam,
        LegacySteam,
        NoValidSteam
    }

    public static SteamPathType GetSteamPath(out string? steamPath)
    {
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        steamPath = Path.Join(homeDirectory, ".local/share/Steam");
        if (IsValidSteamPath(steamPath))
        {
            return SteamPathType.HostInstalledSteam;
        }

        var flatpakSteamPath = Path.Join(homeDirectory, ".var/app/com.valvesoftware.Steam/.local/share/Steam");
        if (IsValidSteamPath(flatpakSteamPath))
        {
            steamPath = flatpakSteamPath;
            return SteamPathType.FlatpakSteam;
        }

        var legacySteamPath = Path.Join(homeDirectory, ".steam/steam");
        if (IsValidSteamPath(legacySteamPath))
        {
            steamPath = legacySteamPath;
            return SteamPathType.LegacySteam;
        }

        steamPath = null;
        return SteamPathType.NoValidSteam;
    }

    private static bool IsValidSteamPath(string path)
    {
        return File.Exists(Path.Join(path, "config/libraryfolders.vdf"));
    }
}