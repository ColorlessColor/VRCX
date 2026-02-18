using System.Text.Json.Serialization;

namespace VRCX.Core.DiscordRpc.Models.Payload;

public sealed record DiscordSetActivityPayload(
    [property: JsonPropertyName("args")] DiscordSetActivityPayloadArgs Args,
    string Nonce
) : DiscordRpcPayloadBase("SET_ACTIVITY", Nonce);

public record DiscordSetActivityPayloadArgs(
    [property: JsonPropertyName("pid")] int ProcessId,
    [property: JsonPropertyName("activity")]
    DiscordRpcActivity? Activity = null
);