using System.Text.Json.Serialization;

namespace VRCX.Core.Models.WebApi;

public sealed record WebApiResponse(
    [property: JsonPropertyName("status")] int StatusCode,
    [property: JsonPropertyName("bodyOrErrorMessage")] string BodyOrErrorMessage
);