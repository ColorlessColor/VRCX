using System;
using System.IO;

namespace VRCX.Core;

public static class AppPathService
{
    public static string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;

    public static string AppDataDirectory { get; set; } = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "VRCX");

    public static string ConfigLocation => Path.Join(AppDataDirectory, "VRCX.sqlite3");

    public static void DoMigrationIfNeeded()
    {
        if (!Directory.Exists(AppDataDirectory))
        {
            Directory.CreateDirectory(AppDataDirectory);

            // Migrate config to AppData
            if (File.Exists(Path.Join(BaseDirectory, "VRCX.json")))
            {
                File.Move(Path.Join(BaseDirectory, "VRCX.json"), Path.Join(AppDataDirectory, "VRCX.json"));
                File.Copy(Path.Join(AppDataDirectory, "VRCX.json"),
                    Path.Join(AppDataDirectory, "VRCX-backup.json"));
            }

            if (File.Exists(Path.Join(BaseDirectory, "VRCX.sqlite3")))
            {
                File.Move(Path.Join(BaseDirectory, "VRCX.sqlite3"),
                    Path.Join(AppDataDirectory, "VRCX.sqlite3"));
                File.Copy(Path.Join(AppDataDirectory, "VRCX.sqlite3"),
                    Path.Join(AppDataDirectory, "VRCX-backup.sqlite3"));
            }
        }

        // Migrate cache to userdata for Cef 115 update
        var oldCachePath = Path.Join(AppDataDirectory, "cache");
        var newCachePath = Path.Join(AppDataDirectory, "userdata", "cache");
        if (Directory.Exists(oldCachePath) && !Directory.Exists(newCachePath))
        {
            Directory.CreateDirectory(Path.Join(AppDataDirectory, "userdata"));
            Directory.Move(oldCachePath, newCachePath);
        }
    }
}