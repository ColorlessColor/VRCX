using System.Text.Json.Serialization;

namespace VRCX.Core.OverlayClient.XsOverlay.Models;

public record XsOverlayWebSocketPayload(
    string Command,
    string? JsonData = null,
    string? RawData = null,
    string Target = "xsoverlay"
);

internal record XsOverlayWebSocketMessage(
    [property: JsonPropertyName("sender")] string Sender,
    [property: JsonPropertyName("command")]
    string Command,
    [property: JsonPropertyName("jsonData")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? JsonData = null,
    [property: JsonPropertyName("rawData")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? RawData = null,
    [property: JsonPropertyName("target")] string Target = "xsoverlay"
);