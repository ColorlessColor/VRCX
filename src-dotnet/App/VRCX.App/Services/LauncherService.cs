using System.Diagnostics;
using System.Text.RegularExpressions;
using Avalonia.Platform.Storage;
using VRCX.Core.Services.Platform;

namespace VRCX.App.Services;

public sealed partial class LauncherService(AppWindowService appWindowService) : IPlatformLauncherService
{
    public async ValueTask<bool> LaunchUriAsync(Uri uri)
    {
        if (appWindowService.GetTopLevel()?.Launcher is { } launcher)
        {
            return await launcher.LaunchUriAsync(uri);
        }

        if (!uri.IsAbsoluteUri)
            return false;

        return Exec(uri.AbsoluteUri);
    }

    public async ValueTask<bool> LaunchFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        if (appWindowService.GetTopLevel() is { } topLevel)
        {
            var launcher = topLevel.Launcher;
            var storageProvider = topLevel.StorageProvider;
            if (await storageProvider.TryGetFileFromPathAsync(filePath) is not { } storageItem)
                return false;

            return await launcher.LaunchFileAsync(storageItem);
        }

        return Exec(filePath);
    }

    #region Fallback

    // https://github.com/AvaloniaUI/Avalonia/blob/54832179448739b2bcb26dab6872e4070bfe1bab/src/Avalonia.Base/Platform/Storage/FileIO/BclLauncher.cs#L40-L90

    private static bool Exec(string urlOrFile)
    {
        if (OperatingSystem.IsLinux())
        {
            // If no associated application/json MimeType is found xdg-open opens return error
            // but it tries to open it anyway using the console editor (nano, vim, other..)
            var args = EscapeForShell(urlOrFile);
            ShellExecRaw($"xdg-open \\\"{args}\\\"", waitForExit: false);
            return true;
        }
        else if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS())
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? urlOrFile : "open",
                Arguments = OperatingSystem.IsMacOS() ? $"{urlOrFile}" : "",
                CreateNoWindow = true,
                UseShellExecute = OperatingSystem.IsWindows()
            });
            return true;
        }
        else
        {
            return false;
        }
    }

    private static string EscapeForShell(string input) => ShllEscapeRegex().Replace(input, "\\")
        .Replace("\"", "\\\\\\\"");

    private static void ShellExecRaw(string cmd, bool waitForExit = true)
    {
        using var process = Process.Start(
            new ProcessStartInfo
            {
                FileName = "/bin/sh",
                Arguments = $"-c \"{cmd}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            }
        );
        if (waitForExit)
        {
            process?.WaitForExit();
        }
    }

    [GeneratedRegex("(?=[`~!#&*()|;'<>])")]
    private static partial Regex ShllEscapeRegex();

    #endregion
}