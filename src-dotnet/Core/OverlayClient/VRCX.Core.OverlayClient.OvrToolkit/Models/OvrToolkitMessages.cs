using System.Text.Json.Serialization;

namespace VRCX.Core.OverlayClient.OvrToolkit.Models;

public sealed class OvrToolkitWebSocketMessage
{
    [JsonPropertyName("messageType")] public required string MessageType { get; set; }
    [JsonPropertyName("json")] public required string Json { get; set; }
}

public sealed class OvrToolkitWebSocketHudNotificationMessage
{
    [JsonPropertyName("title")] public required string Title { get; set; }
    [JsonPropertyName("body")] public required string Body { get; set; }
    [JsonPropertyName("icon")] public required byte[] Icon { get; set; }
}

public sealed class OvrToolkitWebSocketWristNotificationMessage
{
    [JsonPropertyName("body")] public required string Body { get; set; }
}