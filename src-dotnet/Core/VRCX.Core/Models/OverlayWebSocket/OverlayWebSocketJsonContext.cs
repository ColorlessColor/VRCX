using System.Text.Json.Serialization;

namespace VRCX.Core.Models.OverlayWebSocket;

[JsonSerializable(typeof(OverlayMessage))]
internal sealed partial class OverlayWebSocketJsonContext : JsonSerializerContext;