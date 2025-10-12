using System.Text.Json.Serialization;

namespace Reswifty.API.Contracts.Auth.Responses;

public sealed record LoginResponse(
    [property: JsonPropertyName("accessToken")]
    string AccessToken,
    [property: JsonPropertyName("refreshToken")]
    string RefreshToken,
    [property: JsonPropertyName("expiresAtUtc")]
    DateTime ExpiresAtUtc
);
