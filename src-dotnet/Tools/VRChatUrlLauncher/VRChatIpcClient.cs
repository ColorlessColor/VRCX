using System.Buffers;
using System.IO.Pipes;
using System.Text;

namespace VRChatUrlLauncher;

public class VRChatIpcClient
{
    private const string PipeName = "VRChatURLLaunchPipe";

    public static async Task<bool> SendAsync(string message)
    {
        try
        {
            Console.WriteLine("Sending IPC message to VRChat: {0}", message);

            await using var pipeClientStream = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut);
            await pipeClientStream.ConnectAsync(1000);

            var bytes = Encoding.UTF8.GetBytes(message);
            await pipeClientStream.WriteAsync(bytes);

            using var buffer = MemoryPool<byte>.Shared.Rent(1);
            await pipeClientStream.ReadExactlyAsync(buffer.Memory[..1]);

            if (buffer.Memory.Span[0] != 1)
            {
                Console.WriteLine("Failed to send IPC message to VRChat: Return value are not true");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to send IPC message to VRChat: {0}", ex);
            return false;
        }
    }
}