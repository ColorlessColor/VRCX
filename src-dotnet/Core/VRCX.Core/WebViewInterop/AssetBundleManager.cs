using VRCX.Core.Services;

namespace VRCX.Core.WebViewInterop;

public class AssetBundleManager(AssetBundleService assetBundleService)
{
    public List<string> SweepCache() => assetBundleService.SweepCache();
    public long GetCacheSize() => assetBundleService.GetCacheSize();

    public string GetVRChatCacheFullLocation(string id, int version, string variant = "", int variantVersion = 0) =>
        assetBundleService.GetVRChatCacheFullLocation(id, version, variant, variantVersion);

    public Tuple<long, bool, string> CheckVRChatCache(string id, int version, string variant, int variantVersion) =>
        assetBundleService.CheckVRChatCache(id, version, variant, variantVersion);

    public void DeleteCache(string id, int version, string variant, int variantVersion) =>
        assetBundleService.DeleteCache(id, version, variant, variantVersion);

    public void DeleteAllCache() => assetBundleService.DeleteAllCache();
}