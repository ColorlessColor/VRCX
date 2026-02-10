using System.Buffers;
using System.IO.Pipes;
using Serilog;

namespace VRCX.Core.Ipc;

public class VRChatIpcClient
{
    private const string PipeName = "VRChatURLLaunchPipe";

    private static readonly ILogger Logger = Log.ForContext<VRChatIpcClient>();

    public static async Task<bool> SendAsync(string message)
    {
        try
        {
            await using var pipeClientStream = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut);
            await pipeClientStream.ConnectAsync(1000);

            await using var writer = new StreamWriter(pipeClientStream);
            await writer.WriteAsync(message);

            using var buffer = MemoryPool<byte>.Shared.Rent(1);
            var readBytes = await pipeClientStream.ReadAsync(buffer.Memory[..1]);

            if (readBytes == 0)
            {
                Logger.Warning("Failed to send IPC message to VRChat: No bytes received");
                return false;
            }

            if (buffer.Memory.Span[0] != 1)
            {
                Logger.Warning("Failed to send IPC message to VRChat: Return value are not true");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to send IPC message to VRChat: {Message}", message);
            return false;
        }
    }
}