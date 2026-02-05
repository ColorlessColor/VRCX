using System.IO.Pipes;
using NLog;
using VRCX.Core.Models.Ipc;
using VRCX.Core.Services.Ipc;

namespace VRCX.Core.Ipc;

public static class UrlHandlerIpcClient
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static void TrySendUrl(string url)
    {
        try
        {
            using var pipeClientStream = new NamedPipeClientStream(
                ".",
                IpcServerService.GetIpcPipeName(),
                PipeDirection.Out,
                PipeOptions.Asynchronous);

            pipeClientStream.Connect(TimeSpan.FromSeconds(1));

            var payload = new LaunchCommandIpcPacketPayload(url);
            IpcPacket.WriteToStream(payload, pipeClientStream);
        }
        catch (TimeoutException ex)
        {
            Logger.Error(ex, "Timeout connecting to IPC server to send URL: {Url}", url);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error connecting to IPC server to send URL: {Url}", url);
        }
    }
}