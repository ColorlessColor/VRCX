using Microsoft.Win32;
using Serilog;
using VRCX.Core.Services.Platform;

namespace VRCX.Core.Platform.Windows.Services;

public sealed class WindowsStartupSettingsService : IOsStartupSettingsService
{
    private readonly ILogger _logger = Log.ForContext<WindowsStartupSettingsService>();

    public ValueTask EnableAutoLaunchAsync()
    {
        SetAutoLaunchCore(true);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisableAutoLaunchAsync()
    {
        SetAutoLaunchCore(false);
        return ValueTask.CompletedTask;
    }

    private void SetAutoLaunchCore(bool enabled)
    {
        _logger.Information("Setting startup with Windows to {Enabled}", enabled);

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (key == null)
                throw new InvalidOperationException("Failed to open registry key for startup");

            if (enabled)
            {
                var path = Environment.ProcessPath;
                if (path is null)
                    throw new InvalidOperationException("Failed to get Environment.ProcessPath");

                if (!path.EndsWith(".exe"))
                    throw new InvalidOperationException("Environment.ProcessPath is not an executable: " + path);

                key.SetValue("VRCX", $"\"{path}\" --startup");
            }
            else
            {
                key.DeleteValue("VRCX", false);
            }
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Failed to set startup");
        }
    }
}