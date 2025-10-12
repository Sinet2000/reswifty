namespace Reswifty.API.Application.Auth.DTOs;

public sealed record AuthResult(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc,
    int UserId,
    string Email,
    IReadOnlyList<string> Roles
);
