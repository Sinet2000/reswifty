namespace Reswifty.API.Options;

public record ApiOptions
{
    public const string SectionName = "ApiConfig";

    public required string[] AllowedOrigins { get; init; } = [];

    public required string[] SupportedCultures { get; init; } = [];

    public int PermitLimit { get; init; } = 100;

    public int WindowMinutes { get; init; } = 1;

    public int QueueLimit { get; init; } = 2;
}
