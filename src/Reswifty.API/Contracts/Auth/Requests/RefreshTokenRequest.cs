using System.Text.Json.Serialization;

namespace Reswifty.API.Contracts.Auth.Requests;

public sealed record RefreshTokenRequest(
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("userId")] int UserId,
    [property: JsonPropertyName("expiryDate")] DateTime ExpiryDate
);
