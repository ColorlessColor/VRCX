using System.Threading.Tasks;

namespace VRCX.Core.Services.Platform;

public interface IOsStartupSettingsService
{
    ValueTask EnableAutoLaunchAsync();
    ValueTask DisableAutoLaunchAsync();
}