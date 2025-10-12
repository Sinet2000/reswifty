using System.Text.Json.Serialization;

namespace Reswifty.API.Contracts.Auth.Requests;

public sealed record LoginRequest(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password
);
