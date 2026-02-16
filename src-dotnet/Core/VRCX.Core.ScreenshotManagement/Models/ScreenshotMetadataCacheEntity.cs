namespace VRCX.Core.ScreenshotManagement.Models;

public record ScreenshotMetadataCacheEntityDto(
    string FilePath,
    string? Metadata,
    DateTimeOffset CachedAt
);

public record ScreenshotMetadataCacheEntity(
    int Id,
    string FilePath,
    string? Metadata,
    DateTimeOffset CachedAt
) : ScreenshotMetadataCacheEntityDto(FilePath, Metadata, CachedAt);