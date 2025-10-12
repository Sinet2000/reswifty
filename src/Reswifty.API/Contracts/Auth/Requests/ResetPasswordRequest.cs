using System.Text.Json.Serialization;

namespace Reswifty.API.Contracts.Auth.Requests;

public sealed record ResetPasswordRequest(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("newPassword")] string NewPassword
);
