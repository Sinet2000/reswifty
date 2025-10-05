namespace Reswifty.API.Infrastructure.Config;

public record LoggingConfig
{
    public const string SectionName = "LoggingConfig";

    public required SeqLoggingConfig Seq { get; init; } = null!;

    public GrafanaConfig? Grafana { get; init; }

    public DbLoggingConfig? PgSql { get; init; }

    public bool IsWriteToFileEnabled { get; init; }
}

public record SeqLoggingConfig
{
    public required string Url { get; init; } = string.Empty;

    public required string ApiKey { get; init; } = string.Empty;

    public bool Enabled { get; init; }
}

public record GrafanaConfig
{
    public bool Enabled { get; init; }
}

public record DbLoggingConfig
{
    public bool Enabled { get; init; }

    public string? DbConnectionString { get; init; }
}