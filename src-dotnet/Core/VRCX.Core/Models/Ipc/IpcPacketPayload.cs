using System.Text.Json.Serialization;

namespace VRCX.Core.Models.Ipc;

public record IpcPacketPayload(
    [property: JsonPropertyName("type")] string Type
);

public record LaunchCommandIpcPacketPayload(
    [property: JsonPropertyName("command")]
    string Command
) : IpcPacketPayload("LaunchCommand");

public record IpcOutPacketPayload(
    string Type,
    string? Data,
    string? MsgType
);