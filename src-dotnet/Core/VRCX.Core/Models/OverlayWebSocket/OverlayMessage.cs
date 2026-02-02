using System.Text.Json.Serialization;

namespace VRCX.Core.Models.OverlayWebSocket;

public class OverlayMessage
{
    public OverlayMessageType Type { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FunctionName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public string? Data { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public OverlayVars? OverlayVars { get; set; }
}

public enum OverlayMessageType
{
    OverlayConnected,
    JsFunctionCall,
    UpdateVars,
    IsHmdAfk
}