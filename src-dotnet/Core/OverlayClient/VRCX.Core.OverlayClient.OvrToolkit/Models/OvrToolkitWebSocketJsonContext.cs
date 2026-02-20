using System.Text.Json.Serialization;

namespace VRCX.Core.OverlayClient.OvrToolkit.Models;

[JsonSerializable(typeof(OvrToolkitWebSocketMessage))]
[JsonSerializable(typeof(OvrToolkitWebSocketHudNotificationMessage))]
[JsonSerializable(typeof(OvrToolkitWebSocketWristNotificationMessage))]
public sealed partial class OvrToolkitWebSocketJsonContext : JsonSerializerContext;