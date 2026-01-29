namespace VRCX.Core.Utils;

public static class PlayerPrefsUtils
{
    public static string AddHashToKeyName(string key)
    {
        // https://discussions.unity.com/t/playerprefs-changing-the-name-of-keys/30332/4
        // VRC_GROUP_ORDER_usr_032383a7-748c-4fb2-94e4-bcb928e5de6b_h2810492971
        uint hash = 5381;
        foreach (var c in key)
            hash = (hash * 33) ^ c;
        return key + "_h" + hash;
    }
}