namespace VRCX.Core.Utils;

public static class WindowsShortcutUtils
{
    private static readonly byte[] ShortcutSignatureBytes = [0x4C, 0x00, 0x00, 0x00]; // signature for ShellLinkHeader

    /// <summary>
    /// Finds windows shortcut files in a given folder.
    /// </summary>
    /// <param name="folderPath">The folder path.</param>
    /// <returns>An array of shortcut paths. If none, then empty.</returns>
    public static List<string> FindShortcutFiles(string folderPath)
    {
        var directoryInfo = new DirectoryInfo(folderPath);
        var files = directoryInfo.GetFiles();
        var ret = new List<string>();

        foreach (var file in files)
        {
            if (IsShortcutFile(file.FullName))
            {
                ret.Add(file.FullName);
            }
        }

        return ret;
    }

    /// <summary>
    /// Determines whether the specified file path is a shortcut by checking the file header.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <returns><c>true</c> if the given file path is a shortcut, otherwise <c>false</c></returns>
    public static bool IsShortcutFile(string filePath)
    {
        var headerBytes = new byte[4];
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        if (fileStream.Length < 4)
            return false;
        fileStream.ReadExactly(headerBytes, 0, 4);

        return headerBytes.SequenceEqual(ShortcutSignatureBytes);
    }
}