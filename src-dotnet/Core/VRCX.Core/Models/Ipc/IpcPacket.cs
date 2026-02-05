using System.Buffers.Binary;
using System.Text.Json;

namespace VRCX.Core.Models.Ipc;

public static class IpcPacket
{
    public static void WriteToStream<T>(
        T payload,
        Stream stream
    ) where T : IpcPacketPayload
    {
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(payload);

        Span<byte> buffer = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, jsonBytes.Length);

        stream.Write(buffer);
        stream.Write(jsonBytes);
        stream.Flush();
    }

    public static async ValueTask WriteToStreamAsync<T>(
        T payload,
        Stream stream,
        CancellationToken cancellationToken = default
    ) where T : IpcPacketPayload
    {
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(payload);

        Memory<byte> buffer = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(buffer.Span, jsonBytes.Length);

        await stream.WriteAsync(buffer, cancellationToken);
        await stream.WriteAsync(jsonBytes, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }
}