using System.Text.Json.Serialization;
using VRCX.Core.Models.ScreenshotManagement;
using VRCX.Core.ScreenshotManagement.Models;

namespace VRCX.Core.Models;

[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(string[]))]
internal sealed partial class AppCommonJsonContext : JsonSerializerContext;

[JsonSerializable(typeof(ScreenshotMetadata))]
[JsonSerializable(typeof(GetScreenshotMetadataError))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class AppApiScreenshotJsonContext : JsonSerializerContext;