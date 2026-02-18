using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using VRCX.Core.DiscordRpc.Models;
using VRCX.Core.DiscordRpc.Models.Payload;
using VRCX.Core.DiscordRpc.RpcPipe;

namespace VRCX.Core.DiscordRpc;

// https://github.com/yell0wsuit/cuttherope-dx/pull/112
// https://github.com/discord/discord-rpc/blob/master/documentation/hard-mode.md
public sealed partial class SimpleDiscordRpcClient(string clientId) : IDisposable
{
    public string ClientId => clientId;

    private bool _receivedReady;
    public bool IsReady => _receivedReady && _rpcPipeConnection.IsConnected;

    private readonly IDiscordRpcPipeConnection _rpcPipeConnection =
        OperatingSystem.IsWindows() ? new WindowsDiscordRpcPipeConnection() : throw new PlatformNotSupportedException();

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        _receivedReady = false;
        await _rpcPipeConnection.ConnectAsync(cancellationToken);

        try
        {
            var handshakeBytes = JsonSerializer.SerializeToUtf8Bytes(
                new DiscordRpcHandshake(1, clientId),
                DiscordRpcJsonContext.Default.DiscordRpcHandshake
            );

            await SendPacketAsync(DiscordRpcOpCodes.Handshake, handshakeBytes);
            var handshakeResponse = await ReadPacketAsync();

            if (handshakeResponse.OpCode != DiscordRpcOpCodes.Frame ||
                !handshakeResponse.Payload.Contains("\"READY\"", StringComparison.Ordinal))
            {
                await _rpcPipeConnection.DisconnectAsync(CancellationToken.None);
                throw new Exception("Failed to receive valid handshake response from Discord RPC.");
            }

            _receivedReady = true;
        }
        catch
        {
            await _rpcPipeConnection.DisconnectAsync(CancellationToken.None);
        }
    }

    private async Task SendPacketAsync(DiscordRpcOpCodes opCode, byte[] payload)
    {
        var header = new Memory<byte>(new byte[8]);
        BinaryPrimitives.WriteInt32LittleEndian(header[..4].Span, (int)opCode);
        BinaryPrimitives.WriteInt32LittleEndian(header[4..].Span, payload.Length);

        await _rpcPipeConnection.PipeStream.WriteAsync(header);
        await _rpcPipeConnection.PipeStream.WriteAsync(payload);
        await _rpcPipeConnection.PipeStream.FlushAsync();
    }

    private async Task<DiscordRpcPacket> ReadPacketAsync()
    {
        var stream = _rpcPipeConnection.PipeStream;

        var header = new Memory<byte>(new byte[8]);
        await stream.ReadExactlyAsync(header);

        var opcode = (DiscordRpcOpCodes)BinaryPrimitives.ReadInt32LittleEndian(header[..4].Span);
        var length = BinaryPrimitives.ReadInt32LittleEndian(header[4..].Span);

        if (length is <= 0 or > 65536)
        {
            throw new InvalidDataException($"Invalid packet length: {length}");
        }

        var payloadBuffer = MemoryPool<byte>.Shared.Rent(length);
        await stream.ReadExactlyAsync(payloadBuffer.Memory[..length]);

        var payload = Encoding.UTF8.GetString(payloadBuffer.Memory.Span);

        return new DiscordRpcPacket(opcode, payload);
    }

    public void Dispose()
    {
        _rpcPipeConnection.Dispose();
    }
}