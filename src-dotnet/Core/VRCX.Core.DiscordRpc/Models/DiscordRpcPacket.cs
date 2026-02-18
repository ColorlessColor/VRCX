namespace VRCX.Core.DiscordRpc.Models;

internal record DiscordRpcPacket(DiscordRpcOpCodes OpCode, string Payload);