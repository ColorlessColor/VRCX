using System.Text.Json.Serialization;

namespace VRCX.Core.DiscordRpc.Models.Payload;

internal sealed record DiscordRpcHandshake(
    [property: JsonPropertyName("v")] int Version,
    [property: JsonPropertyName("client_id")]
    string ClientId
);