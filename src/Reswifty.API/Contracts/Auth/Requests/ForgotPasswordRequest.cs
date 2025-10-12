using System.Text.Json.Serialization;

namespace Reswifty.API.Contracts.Auth.Requests;

public sealed record ForgotPasswordRequest(
    [property: JsonPropertyName("email")] string Email
);
