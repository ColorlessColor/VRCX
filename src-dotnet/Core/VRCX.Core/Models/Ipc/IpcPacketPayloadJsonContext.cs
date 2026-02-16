using System.Text.Json.Serialization;

namespace VRCX.Core.Models.Ipc;

[JsonSerializable(typeof(IpcPacketPayload))]
[JsonSerializable(typeof(LaunchCommandIpcPacketPayload))]
[JsonSerializable(typeof(IpcOutPacketPayload))]
public sealed partial class IpcPacketPayloadJsonContext : JsonSerializerContext;