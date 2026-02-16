using System.Text.Json;
using System.Text.Json.Serialization;

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
        _logger.Information("Loading update status from {UpdateStatusFilePath}", UpdateStatusFilePath);

        if (!File.Exists(UpdateStatusFilePath))
            return null;

        var json = await File.ReadAllTextAsync(UpdateStatusFilePath);
        return JsonSerializer.Deserialize<UpdateStatusRecord>(json, UpdateStatusJsonContext.Default.UpdateStatusRecord);
    }

    private async ValueTask SaveUpdateStatus(string targetVersion)
    {
        _logger.Information("Saving update status for target version {TargetVersion} to {UpdateStatusFilePath}",
            targetVersion,
            UpdateStatusFilePath);

        var record = new UpdateStatusRecord(targetVersion);
        var json = JsonSerializer.Serialize(record, UpdateStatusJsonContext.Default.UpdateStatusRecord);
        await File.WriteAllTextAsync(UpdateStatusFilePath, json);
    }

    private void ClearUpdateStatus()
    {
        _logger.Information("Clearing update status file at {UpdateStatusFilePath}", UpdateStatusFilePath);
        if (File.Exists(UpdateStatusFilePath))
            File.Delete(UpdateStatusFilePath);
    }

    private record UpdateStatusRecord(string TargetVersion);

    [JsonSerializable(typeof(UpdateStatusRecord))]
    private sealed partial class UpdateStatusJsonContext : JsonSerializerContext;
}