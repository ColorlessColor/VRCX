using System.Diagnostics;
using VRCX.Core.Services.Platform;

namespace VRCX.LegacyApp.WinFormsCef.CoreGlue.Services.Platform;

public sealed class WinFormsLauncherService : IPlatformLauncherService
{
    public ValueTask<bool> LaunchUriAsync(Uri uri)
    {
        if (!uri.IsAbsoluteUri)
            return ValueTask.FromResult(false);

        return ValueTask.FromResult(Exec(uri.AbsoluteUri));
    }

    public ValueTask<bool> LaunchFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return ValueTask.FromResult(false);

        return ValueTask.FromResult(Exec(filePath));
    }

    private static bool Exec(string urlOrFile)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = urlOrFile,
            CreateNoWindow = true,
            UseShellExecute = true
        });
        return true;
    }
}