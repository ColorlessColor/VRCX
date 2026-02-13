using System.Text.Json.Serialization;

namespace VRCX.Core.OverlayClient.XsOverlay.Models.CommandPayload;

public record XsOverlayWebSocketNotification(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("message")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Message,
    [property: JsonPropertyName("useBase64Icon")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    bool? UseBase64Icon,
    [property: JsonPropertyName("icon")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Icon,
    [property: JsonPropertyName("timeout")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    float? Timeout = null,
    [property: JsonPropertyName("height")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    float? Height = null,
    [property: JsonPropertyName("opacity")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    float? Opacity = null,
    [property: JsonPropertyName("volume")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    float? Volume = null,
    [property: JsonPropertyName("audioPath")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? AudioPath = null,
    [property: JsonPropertyName("sourceApp")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? SourceApp = null,
    [property: JsonPropertyName("type")] int Type = 1
);