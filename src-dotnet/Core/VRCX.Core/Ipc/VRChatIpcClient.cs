using System.Buffers;
using System.IO.Pipes;
using System.Text;
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
            Logger.Information("Sending IPC message to VRChat: {Message}", message);

            await using var pipeClientStream = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut);
            await pipeClientStream.ConnectAsync(1000);

            var bytes = Encoding.UTF8.GetBytes(message);
            await pipeClientStream.WriteAsync(bytes);

            using var buffer = MemoryPool<byte>.Shared.Rent(1);
            await pipeClientStream.ReadExactlyAsync(buffer.Memory[..1]);

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