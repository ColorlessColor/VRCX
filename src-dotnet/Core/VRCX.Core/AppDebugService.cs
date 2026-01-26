namespace VRCX.Core;

public static class AppDebugService
{
    public static bool InDebugMode { get; set; } = IsDebugBuild();

    private static bool IsDebugBuild()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}