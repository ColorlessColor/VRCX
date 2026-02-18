using System.Text.Json.Serialization;

namespace VRCX.Core.DiscordRpc.Models.Payload;

public abstract record DiscordRpcPayloadBase(
    [property: JsonPropertyName("cmd")] string Command,
    [property: JsonPropertyName("nonce")] string Nonce
);