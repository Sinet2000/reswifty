using System.Text.Json.Serialization;

namespace Reswifty.API.Contracts.Auth.Requests;

public sealed record ConfirmEmailRequest(
    [property: JsonPropertyName("userId")] string UserId,
    [property: JsonPropertyName("token")] string Token
);
