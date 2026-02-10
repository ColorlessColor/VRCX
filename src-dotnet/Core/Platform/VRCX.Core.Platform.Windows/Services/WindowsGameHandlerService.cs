using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Win32;
using Serilog;
using VRCX.Core.Services.Platform;
using VRCX.Core.Utils;

namespace VRCX.Core.Platform.Windows.Services;

public sealed partial class WindowsGameHandlerService : IGameHandlerService
{
    private readonly ILogger _logger = Log.ForContext<WindowsGameHandlerService>();

    public ValueTask<int> QuitGameAsync()
    {
        var processes = Process.GetProcessesByName(VRChatUtils.VRChatProcessName);
        if (processes.Length == 1)
            processes[0].Kill();

        foreach (var process in processes)
            process.Dispose();

        return ValueTask.FromResult(processes.Length);
    }

    #region Launch Game

    public async ValueTask<bool> LaunchGameAsync(string arguments)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = $"steam://run/{VRChatUtils.VRChatSteamAppid}//{HttpUtility.UrlEncode(arguments)}/",
                CreateNoWindow = true,
                UseShellExecute = true
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to launch VRChat via Steam. Falling back to registry method");
        }

        return await LaunchGameFromRegisterAsync(arguments);
    }

    public ValueTask<bool> LaunchGameFromPathAsync(string gamePath, string arguments)
    {
        if (File.Exists(gamePath))
        {
            _logger.Error("Failed to launch VRChat from path: File not found - {GamePath}", gamePath);
            return ValueTask.FromResult(false);
        }

        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(gamePath),
                FileName = gamePath,
                Arguments = arguments,
            });

            return ValueTask.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to launch VRChat from path: {GamePath}", gamePath);
            return ValueTask.FromResult(false);
        }
    }

    private async ValueTask<bool> LaunchGameFromRegisterAsync(string arguments)
    {
        const string keyPath = @"VRChat\shell\open\command";
        using var key = Registry.ClassesRoot.OpenSubKey(keyPath);
        if (key is null)
        {
            _logger.Error("Failed to launch VRChat from register: HKCR\\{RegisterKey} not found", keyPath);
            return false;
        }

        if (key.GetValueKind("") != RegistryValueKind.String)
        {
            _logger.Error("Failed to launch VRChat from register: HKCR\\{RegisterKey} is not String", keyPath);
            return false;
        }

        if (key.GetValue("") is not string keyValue || string.IsNullOrWhiteSpace(keyValue))
        {
            _logger.Error("Failed to launch VRChat from register: HKCR\\{RegisterKey} value is empty", keyPath);
            return false;
        }

        if (TryParseExecutablePathFromRegistryValue(keyValue) is null)
        {
            _logger.Error("Failed to launch VRChat from register: HKCR\\{RegisterKey} value is invalid", keyPath);
            return false;
        }

        return await LaunchGameFromPathAsync(keyPath, arguments);
    }

    private static string? TryParseExecutablePathFromRegistryValue(string keyValue)
    {
        if (keyValue.StartsWith('"'))
            return keyValue;

        var regex = ExecutablePathRegex();
        var match = regex.Match(keyValue);
        if (!match.Success)
            return null;

        return match.Groups["ExePath"].Value;
    }

    [GeneratedRegex("""(?:^"(?<ExePath>.+?)")""")]
    private static partial Regex ExecutablePathRegex();

    #endregion
}