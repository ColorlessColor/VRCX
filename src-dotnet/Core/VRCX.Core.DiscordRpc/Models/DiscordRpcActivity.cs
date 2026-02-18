using System.Text.Json.Serialization;

namespace VRCX.Core.DiscordRpc.Models;

public sealed record DiscordRpcActivity
{
    [JsonPropertyName("details")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Details { get; set; }

    [JsonPropertyName("details_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DetailsUrl { get; set; }

    [JsonPropertyName("state")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? State { get; set; }

    [JsonPropertyName("timestamps")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DiscordRpcActivityTimestamps? Timestamps { get; set; }

    [JsonPropertyName("party")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DiscordRpcActivityParty? Party { get; set; }

    [JsonPropertyName("assets")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DiscordRpcActivityAssets? Assets { get; set; }

    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DiscordRpcActivityType? Type { get; set; }

    [JsonPropertyName("status_display_type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DiscordRpcActivityStatusDisplayType? StatusDisplayType { get; set; }

    [JsonPropertyName("buttons")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DiscordRpcActivityButton[]? Buttons { get; set; }
}

public sealed record DiscordRpcActivityButton
{
    [JsonPropertyName("label")] public required string Label { get; set; }
    [JsonPropertyName("url")] public required string Url { get; set; }
}

public sealed record DiscordRpcActivityParty
{
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; set; }

    [JsonPropertyName("size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    private int[]? Size
    {
        get => [Current, Max];
        set
        {
            if (value == null || value.Length != 2)
            {
                Current = 0;
                Max = 0;
                return;
            }

            Current = value[0];
            Max = value[1];
        }
    }

    [JsonIgnore] public int Current { get; set; }
    [JsonIgnore] public int Max { get; set; }
}

public sealed record DiscordRpcActivityAssets
{
    [JsonPropertyName("large_image")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LargeImageKey { get; set; }

    [JsonPropertyName("large_text")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LargeImageText { get; set; }

    [JsonPropertyName("large_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LargeImageUrl { get; set; }

    [JsonPropertyName("small_image")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SmallImageKey { get; set; }

    [JsonPropertyName("small_text")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SmallImageText { get; set; }
}

public sealed record DiscordRpcActivityTimestamps
{
    [JsonPropertyName("start")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? StartUnixMilliseconds { get; set; }

    [JsonPropertyName("end")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? EndUnixMilliseconds { get; set; }
}

public enum DiscordRpcActivityType
{
    Playing = 0,
    Listening = 2,
    Watching = 3,
    Competing = 5
}

public enum DiscordRpcActivityStatusDisplayType
{
    Name = 0,
    State = 1,
    Details = 2,
}