using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Reswifty.API.Application.Abstractions.Identity;
using Reswifty.API.Application.Auth.DTOs;
using Reswifty.API.Common.Exceptions;
using Reswifty.API.Domain.Identity;
using Reswifty.API.Infrastructure.Persistence;
using Reswifty.API.Options;

namespace Reswifty.API.Application.Auth.Commands;

public sealed record LoginCommand(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<AuthResult>;

internal sealed class LoginCommandHandler(
    UserManager<User> userManager,
    RoleManager<UserRole> roleManager,
    IJwtTokenService jwt,
    IOptions<AuthenticationOptions> authOptions,
    AppDbContext db
) : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly JwtOptions _jwt = authOptions.Value.Jwt
                                       ?? throw new ArgumentNullException(nameof(authOptions));

    private readonly TimeSpan _accessTtl =
        TimeSpan.FromMinutes(authOptions.Value.AccessTokenMinutes);

    private readonly TimeSpan _refreshTtl =
        TimeSpan.FromDays(authOptions.Value.RefreshTokenDays);

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim();

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            // (soft delay to avoid user enumeration timing)
            await Task.Delay(Random.Shared.Next(150, 350), ct);
            throw new InvalidCredentialsException();
        }

        if (!user.EmailConfirmed)
            throw new AccountEmailNotConfirmedException();

        if (await userManager.IsLockedOutAsync(user))
            throw new AccountLockedException();

        var passwordOk = await userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordOk)
        {
            await userManager.AccessFailedAsync(user);
            throw new InvalidCredentialsException();
        }

        await userManager.ResetAccessFailedCountAsync(user);

        // compose claims
        var claims = await BuildClaimsAsync(user, request);

        // issue access token
        var now = DateTime.UtcNow;
        var accessExp = now.Add(_accessTtl);
        var accessToken = jwt.GenerateToken(claims, now, accessExp);

        // create refresh token (opaque, url-safe), store hashed in DB
        var rawRefresh = GenerateUrlSafeToken(64);
        var tokenHash = jwt.HashToken(rawRefresh, out var salt);
        var refreshExp = now.Add(_refreshTtl);

        var rt = new AuthRefreshToken(
            userId: user.Id,
            hash: tokenHash,
            salt: salt,
            expiresAtUtc: refreshExp,
            ipAddress: request.IpAddress,
            userAgent: request.UserAgent);

        db.RefreshTokens.Add(rt);
        await db.SaveChangesAsync(ct);

        var roles = await userManager.GetRolesAsync(user);

        return new AuthResult(
            AccessToken: accessToken,
            RefreshToken: rawRefresh,
            ExpiresAtUtc: accessExp,
            UserId: user.Id,
            Email: user.Email!,
            Roles: roles.AsReadOnly()
        );
    }

    private async Task<IEnumerable<Claim>> BuildClaimsAsync(User user, LoginCommand req)
    {
        var userClaims = await userManager.GetClaimsAsync(user);
        var roles = await userManager.GetRolesAsync(user);

        var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r));
        var permClaims = new List<Claim>();
        foreach (var roleName in roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null) continue;
            permClaims.AddRange(await roleManager.GetClaimsAsync(role));
        }

        var baseClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()), new(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        if (!string.IsNullOrWhiteSpace(req.IpAddress))
            baseClaims.Add(new("ip", req.IpAddress!));
        if (!string.IsNullOrWhiteSpace(req.UserAgent))
            baseClaims.Add(new("ua", req.UserAgent!));

        // you can also add issuer/audience specific claims if you need
        return baseClaims.Concat(userClaims).Concat(roleClaims).Concat(permClaims);
    }

    // URL-safe base64 without padding so it can travel in headers/URLs safely
    private static string GenerateUrlSafeToken(int bytes)
    {
        var buffer = new byte[bytes];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToBase64String(buffer)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
