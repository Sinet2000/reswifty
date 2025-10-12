using System.ComponentModel.DataAnnotations;

namespace Reswifty.API.Domain.Identity;

public sealed class AuthRefreshToken
{
    private AuthRefreshToken()
    {
    }

    public AuthRefreshToken(
        int userId,
        string hash,
        string salt,
        DateTime expiresAtUtc,
        string? ipAddress = null,
        string? userAgent = null)
    {
        ArgumentNullException.ThrowIfNull(hash);
        ArgumentNullException.ThrowIfNull(salt);

        if (hash.Length > 512) throw new ArgumentException("TokenHash too long");
        if (salt.Length > 256) throw new ArgumentException("TokenSalt too long");

        Id = Guid.NewGuid();
        UserId = userId;
        Hash = hash;
        Salt = salt;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAt = DateTime.UtcNow;
        CreatedByIp = ipAddress?.Trim();
        UserAgent = userAgent?.Trim();
    }

    public Guid Id { get; private set; }

    [Required]
    public int UserId { get; private set; }

    [Required]
    public User User { get; private set; } = null!;

    [Required, MaxLength(512)]
    public string Hash { get; private set; } = null!;

    [Required, MaxLength(512)]
    public string Salt { get; private set; } = null!;

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime CreatedAt { get; private set; }

    [MaxLength(64)]
    public string? CreatedByIp { get; private set; }

    [MaxLength(256)]
    public string? UserAgent { get; private set; }

    public bool IsRevoked { get; private set; } = false;

    public DateTime? RevokedAt { get; private set; }

    [MaxLength(64)]
    public string? RevokedByIp { get; private set; }

    [MaxLength(512)]
    public string? ReplacedByToken { get; private set; }

    public bool IsActive => !IsRevoked && DateTime.UtcNow <= ExpiresAtUtc;

    // DDD-style behaviors
    public void Revoke(string? revokedByIp = null, string? replacedByToken = null)
    {
        if (IsRevoked) return;

        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp?.Trim();
        ReplacedByToken = replacedByToken?.Trim();
    }
}
