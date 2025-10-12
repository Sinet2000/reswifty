using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Reswifty.API.Application.Abstractions.Identity;

namespace Reswifty.API.Infrastructure.Identity.Services;

public class JwtTokenService(string secretKey, string? issuer = null, string? audience = null) : IJwtTokenService
{
    // JWT issuer - used to validate who issued the token
    // JWT audience - used to validate who the token is intended for

    public string GenerateToken(IEnumerable<Claim> claims, DateTime issuedAt, DateTime expiresAt)
    {
        var key = Encoding.UTF8.GetBytes(secretKey);
        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: issuedAt,
            expires: expiresAt,
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validates a JWT string: checks signature, expiry, issuer, audience, etc.
    /// Returns user ClaimsPrincipal if valid, or null if invalid
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token, out IDictionary<string, string>? extractedClaims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);

        extractedClaims = null;
        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
                ValidIssuer = issuer,
                ValidateAudience = !string.IsNullOrWhiteSpace(audience),
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // eliminate default 5 min skew
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            extractedClaims = principal.Claims
                .GroupBy(c => c.Type)
                .ToDictionary(g => g.Key, g => g.First().Value);

            return principal;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Hashes the token using HMACSHA256 and a generated random salt
    /// This lets us store token "fingerprints" safely in DB
    /// </summary>
    public string HashToken(string token, out string salt)
    {
        // Generate random salt
        salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

        using var hmac = new HMACSHA256(Convert.FromBase64String(salt));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(token));

        return Convert.ToBase64String(hash);
    }
}
