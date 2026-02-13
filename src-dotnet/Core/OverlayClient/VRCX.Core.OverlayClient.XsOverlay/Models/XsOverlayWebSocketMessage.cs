using System.Text.Json.Serialization;

namespace VRCX.Core.OverlayClient.XsOverlay.Models;

public record XsOverlayWebSocketPayload(
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

internal record XsOverlayWebSocketMessage(
    [property: JsonPropertyName("sender")] string Sender,
    string Target,
    string Command,
    string? JsonData,
    string? RawData
) : XsOverlayWebSocketPayload(Command, JsonData, RawData, Target);