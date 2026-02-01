namespace VRCX.Core.Services.Platform;

public interface IPlatformLifetimeService
{
    ValueTask InvokeShutdownAsync();
    ValueTask InvokeRestartAsync();
}