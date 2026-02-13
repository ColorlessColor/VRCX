using System.Text.Json.Serialization;
using VRCX.Core.OverlayClient.XsOverlay.Models.CommandPayload;

namespace VRCX.Core.OverlayClient.XsOverlay.Models;

[JsonSerializable(typeof(XsOverlayWebSocketMessage))]
[JsonSerializable(typeof(XsOverlayWebSocketNotification))]
internal sealed partial class XsOverlayWebSocketJsonContext : JsonSerializerContext;