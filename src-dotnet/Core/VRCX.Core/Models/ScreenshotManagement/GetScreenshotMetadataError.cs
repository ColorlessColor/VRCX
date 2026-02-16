using System.Text.Json.Serialization;

namespace VRCX.Core.Models.ScreenshotManagement;

public record GetScreenshotMetadataError(
    [property: JsonPropertyName("sourceFile")]
    string SourceFilePath,
    [property: JsonPropertyName("error")] string ErrorMessage
);