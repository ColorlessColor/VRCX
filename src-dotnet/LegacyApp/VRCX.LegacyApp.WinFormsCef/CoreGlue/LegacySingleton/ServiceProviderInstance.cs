using Microsoft.Extensions.DependencyInjection;

namespace VRCX.LegacyApp.WinFormsCef.CoreGlue.LegacySingleton;

public static class ServiceProviderInstance
{
    public static ServiceProvider Instance { get; set; }
}