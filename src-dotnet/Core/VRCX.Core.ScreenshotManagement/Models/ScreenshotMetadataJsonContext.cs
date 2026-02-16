using System.Text.Json.Serialization;

namespace VRCX.Core.ScreenshotManagement.Models;

[JsonSerializable(typeof(ScreenshotMetadata))]
internal partial class ScreenshotMetadataJsonContext : JsonSerializerContext;