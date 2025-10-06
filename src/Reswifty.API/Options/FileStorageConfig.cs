using Dexlaris.Core.Exceptions;

namespace Reswifty.API.Options;

public record FileStorageConfig
{
    public const string SectionName = "FileStorageConfig";

    public required string BasePath { get; init; } = string.Empty;

    public required string JwtEncryptionKey { get; init; } = string.Empty;

    public string? BasePathWin { get; init; }

    public long MaxFileSize { get; init; } = 500L * 1024 * 1024;

    public Dictionary<string, long> MaxSizePerDirectory { get; init; }

    public string[] AllowedFileExtensions { get; init; } =
    [
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv", ".txt",
        ".heic", ".jpg", ".jpeg", ".png", ".webp",
        ".mp4", ".mp3", ".ogg",
        ".json", ".xml",
        ".asice", ".edoc"
    ];

    public Dictionary<string, string[]>? AllowedExtensionsPerDirectory { get; init; }

    public int MaxFileUploadCount { get; init; } = 3;

    public int ParallelProcessBatchSize { get; init; } = 30;

    public int FileMaxStoreDays { get; init; } = 30;

    public string GetBasePath()
    {
        if (!OperatingSystem.IsWindows()) return BasePath;

        ContextualArgumentException.ThrowIfNullOrWhiteSpace(BasePathWin);

        return BasePathWin;
    }

    public long GetMaxSizeForDirectory(string directory)
        => MaxSizePerDirectory.TryGetValue(directory, out var size) ? size : MaxFileSize;

    public string[] GetAllowedExtensionsForDirectory(string directory)
        => AllowedExtensionsPerDirectory?.TryGetValue(directory, out var exts) == true ? exts : AllowedFileExtensions;
}