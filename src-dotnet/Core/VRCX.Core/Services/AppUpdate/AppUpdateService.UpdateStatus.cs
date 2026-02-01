using System.Text.Json;

namespace VRCX.Core.Services.AppUpdate;

public sealed partial class AppUpdateService
{
    public async ValueTask<string?> GetInProgressUpdateTargetVersionAsync()
    {
        var status = await LoadUpdateStatus();
        return status?.TargetVersion;
    }

    private async ValueTask<UpdateStatusRecord?> LoadUpdateStatus()
    {
        _logger.Info("Loading update status from {UpdateStatusFilePath}", UpdateStatusFilePath);

        if (!File.Exists(UpdateStatusFilePath))
            return null;

        var json = await File.ReadAllTextAsync(UpdateStatusFilePath);
        return JsonSerializer.Deserialize<UpdateStatusRecord>(json);
    }

    private async ValueTask SaveUpdateStatus(string targetVersion)
    {
        _logger.Info("Saving update status for target version {TargetVersion} to {UpdateStatusFilePath}",
            targetVersion,
            UpdateStatusFilePath);

        var record = new UpdateStatusRecord(targetVersion);
        var json = JsonSerializer.Serialize(record);
        await File.WriteAllTextAsync(UpdateStatusFilePath, json);
    }

    private void ClearUpdateStatus()
    {
        _logger.Info("Clearing update status file at {UpdateStatusFilePath}", UpdateStatusFilePath);
        if (File.Exists(UpdateStatusFilePath))
            File.Delete(UpdateStatusFilePath);
    }

    private record UpdateStatusRecord(string TargetVersion);
}