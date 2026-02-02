using System.Text.Json.Serialization;

namespace VRCX.Core.Models.Ipc;

public record IpcPacket(
    [property: JsonPropertyName("type")] string Type
);

public record LaunchCommandIpcPacket(
    [property: JsonPropertyName("command")]
    string Command
) : IpcPacket("LaunchCommand");

public record IpcOutPacket(
    string Type,
    string? Data,
    string? MsgType
);