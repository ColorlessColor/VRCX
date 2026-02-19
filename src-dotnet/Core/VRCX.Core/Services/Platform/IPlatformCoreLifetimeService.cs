namespace VRCX.Core.Services.Platform;

public interface IPlatformCoreLifetimeService
{
    Task StartAsync();
    Task StopAsync();
}