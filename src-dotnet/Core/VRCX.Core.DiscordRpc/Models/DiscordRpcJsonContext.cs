using System.Text.Json.Serialization;
using VRCX.Core.DiscordRpc.Models.Payload;

namespace VRCX.Core.DiscordRpc.Models;

[JsonSerializable(typeof(DiscordRpcHandshake))]
[JsonSerializable(typeof(DiscordSetActivityPayload))]
internal sealed partial class DiscordRpcJsonContext : JsonSerializerContext;