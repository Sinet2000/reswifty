using System.Security.Claims;
using System.Security.Cryptography;
using Dexlaris.Core.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Reswifty.API.Application.Abstractions.Identity;
using Reswifty.API.Application.Auth.DTOs;
using Reswifty.API.Common.Exceptions.Auth;
using Reswifty.API.Domain.Identity;
using Reswifty.API.Infrastructure.Persistence;
using Reswifty.API.Options;

namespace Reswifty.API.Application.Auth.Commands;

public sealed record RefreshTokenCommand(
    string RefreshToken,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<AuthResult>;

internal sealed class RefreshTokenCommandHandler(
    AppDbContext db,
    UserManager<User> users,
    RoleManager<UserRole> roles,
    IJwtTokenService jwt,
    IOptions<AuthenticationOptions> authOpts
) : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly TimeSpan _accessTtl = TimeSpan.FromMinutes(authOpts.Value.AccessTokenMinutes);
    private readonly TimeSpan _refreshTtl = TimeSpan.FromDays(authOpts.Value.RefreshTokenDays);

    public async Task<AuthResult> Handle(RefreshTokenCommand cmd, CancellationToken ct)
    {
        // 1) Parse "{tokenId}.{secret}"
        if (!TrySplit(cmd.RefreshToken, out var tokenId, out var secret))
            throw new InvalidRefreshTokenException();

        // 2) Load stored token by Id
        var rt = await db.RefreshTokens.AsTracking().FirstOrDefaultAsync(x => x.Id == tokenId, ct);
        if (rt is null) throw new InvalidRefreshTokenException();

        // 3) Basic checks
        if (rt.IsRevoked) throw new RefreshTokenRevokedException();
        if (rt.ExpiresAtUtc <= DateTime.UtcNow) throw new RefreshTokenExpiredException();

        // 4) Verify secret with stored salt (HMACSHA512)
        var computed = HashWithSalt(secret, rt.Salt);
        if (!TimeConstantEquals(computed, rt.Hash))
            throw new InvalidRefreshTokenException();

        // 5) Load user
        var user = await users.FindByIdAsync(rt.UserId.ToString());
        if (user is null || !user.EmailConfirmed) throw new AccountUnavailableException();

        // 6) Rotate refresh token: revoke old + create new
        rt.Revoke(revokedByIp: cmd.IpAddress); // sets IsRevoked/RevokedAt/RevokedByIp

        var (rawRefresh, newRow) = GenerateRefreshTokenRow(
            userId: rt.UserId,
            ip: cmd.IpAddress,
            ua: cmd.UserAgent,
            lifetime: _refreshTtl);

        rt.ReplacedByToken = newRow.Id.ToString();
        db.RefreshTokens.Add(newRow);

        // 7) Issue new access token
        var claims = await BuildClaimsAsync(user);
        var now = DateTime.UtcNow;
        var accessExp = now.Add(_accessTtl);
        var access = jwt.GenerateToken(claims, now, accessExp);

        await db.SaveChangesAsync(ct);

        var roleList = await users.GetRolesAsync(user);

        return new AuthResult(
            AccessToken: access,
            RefreshToken: rawRefresh,
            ExpiresAtUtc: accessExp,
            UserId: user.Id,
            Email: user.Email!,
            Roles: roleList.AsReadOnly()
        );
    }

    // —— helpers ——

    private static bool TrySplit(string token, out Guid tokenId, out string secret)
    {
        tokenId = Guid.Empty;
        secret = "";
        var parts = token.Split('.', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return false;
        return Guid.TryParse(parts[0], out tokenId) && !string.IsNullOrWhiteSpace(secret = parts[1]);
    }

    private async Task<IEnumerable<Claim>> BuildClaimsAsync(User user)
    {
        var userClaims = await users.GetClaimsAsync(user);
        var roleNames = await users.GetRolesAsync(user);
        var roleClaims = roleNames.Select(r => new Claim(ClaimTypes.Role, r));

        var permClaims = new List<Claim>();
        foreach (var rn in roleNames)
        {
            var role = await roles.FindByNameAsync(rn);
            if (role is null) continue;
            permClaims.AddRange(await roles.GetClaimsAsync(role));
        }

        var baseClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()), new(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        return baseClaims.Concat(userClaims).Concat(roleClaims).Concat(permClaims);
    }

    private static (string Raw, AuthRefreshToken Row) GenerateRefreshTokenRow(
        int userId, string? ip, string? ua, TimeSpan lifetime)
    {
        var secret = GenerateUrlSafe(64);
        var (hash, salt) = HashWithNewSalt(secret);

        var row = new AuthRefreshToken(
            userId: userId,
            hash: hash,
            salt: salt,
            expiresAtUtc: DateTime.UtcNow.Add(lifetime),
            ipAddress: ip,
            userAgent: ua);

        var raw = $"{row.Id}.{secret}";
        return (raw, row);
    }

    private static string GenerateUrlSafe(int bytes)
    {
        var buf = new byte[bytes];
        RandomNumberGenerator.Fill(buf);
        return Convert.ToBase64String(buf).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private static (string Hash, string Salt) HashWithNewSalt(string secret)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(32);
        using var hmac = new HMACSHA512(saltBytes);
        var hash = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(secret)));
        var salt = Convert.ToBase64String(saltBytes);
        return (hash, salt);
    }

    private static string HashWithSalt(string secret, string saltB64)
    {
        var saltBytes = Convert.FromBase64String(saltB64);
        using var hmac = new HMACSHA512(saltBytes);
        return Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(secret)));
    }

    private static bool TimeConstantEquals(string aB64, string bB64)
    {
        var a = Convert.FromBase64String(aB64);
        var b = Convert.FromBase64String(bB64);
        if (a.Length != b.Length) return false;
        var diff = 0;
        for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
        return diff == 0;
    }
}
