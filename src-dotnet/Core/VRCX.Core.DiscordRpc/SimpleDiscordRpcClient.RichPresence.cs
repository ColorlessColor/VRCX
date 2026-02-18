using System.Text.Json;
using VRCX.Core.DiscordRpc.Models;
using VRCX.Core.DiscordRpc.Models.Payload;

namespace VRCX.Core.DiscordRpc;

public partial class SimpleDiscordRpcClient
{
    public async Task SetActivityAsync(DiscordRpcActivity activity)
    {
        try
        {
            var rpcPayload = new DiscordSetActivityPayload(
                new DiscordSetActivityPayloadArgs(Environment.ProcessId, activity), Guid.NewGuid().ToString());

            var payload =
                JsonSerializer.SerializeToUtf8Bytes(rpcPayload,
                    DiscordRpcJsonContext.Default.DiscordSetActivityPayload);

            await SendPacketAsync(DiscordRpcOpCodes.Frame, payload);
        }
        catch
        {
            await _rpcPipeConnection.DisconnectAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task ClearActivityAsync()
    {
        try
        {
            var rpcPayload = new DiscordSetActivityPayload(
                new DiscordSetActivityPayloadArgs(Environment.ProcessId), Guid.NewGuid().ToString());

            var payload =
                JsonSerializer.SerializeToUtf8Bytes(rpcPayload,
                    DiscordRpcJsonContext.Default.DiscordSetActivityPayload);

            await SendPacketAsync(DiscordRpcOpCodes.Frame, payload);
        }
        catch
        {
            await _rpcPipeConnection.DisconnectAsync(CancellationToken.None);
            throw;
        }
    }
}