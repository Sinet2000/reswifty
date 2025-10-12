namespace Reswifty.API.Options;

public sealed record AuthenticationOptions
{
    public const string SectionName = "Auth";

    public IdentityOptions? DefaultIdentity { get; init; }

    public required JwtOptions Jwt { get; init; }

    public required int AccessTokenMinutes { get; init; } = 120;

    public required int RefreshTokenDays { get; init; } = 7;
}

public sealed record IdentityOptions
{
    public const string SectionName = "IdentityConfiguration";

    public required string DefaultPassword { get; init; }

    public int InviteExpireDays { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string Email { get; init; }

    public required string Username { get; init; }
}

public sealed record JwtOptions
{
    public const string SectionName = "JWT";

    public required string Secret { get; init; }

    public required string Issuer { get; init; }

    public required string Audience { get; init; }

    public string? AuthPage { get; init; }
}
