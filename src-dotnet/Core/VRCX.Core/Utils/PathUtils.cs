namespace VRCX.Core.Utils;

public static class PathUtils
{
    // https://stackoverflow.com/a/21058121
    public static string NormalizePath(string path)
    {
        return Path.GetFullPath(new Uri(path).LocalPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .ToUpperInvariant();
    }
}