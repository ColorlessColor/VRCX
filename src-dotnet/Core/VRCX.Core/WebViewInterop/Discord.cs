using VRCX.Core.Services;

namespace VRCX.Core.WebViewInterop;

public class Discord(DiscordService discordService)
{
    public void SetAssets(
        string details,
        string state,
        string detailsUrl,
        string largeKey,
        string largeText,
        string smallKey,
        string smallText,
        double startUnixMilliseconds,
        double endUnixMilliseconds,
        string partyId,
        int partySize,
        int partyMax,
        string buttonText,
        string buttonUrl,
        string appId,
        int activityType,
        int statusDisplayType) => discordService.SetAssets(details, state, detailsUrl, largeKey, largeText, smallKey,
        smallText, startUnixMilliseconds, endUnixMilliseconds, partyId, partySize, partyMax, buttonText, buttonUrl,
        appId, activityType, statusDisplayType);

    public bool SetActive(bool active) => discordService.SetActive(active);
}