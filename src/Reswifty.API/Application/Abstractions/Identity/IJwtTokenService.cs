using System.Security.Claims;

namespace Reswifty.API.Application.Abstractions.Identity;

public interface IJwtTokenService
{
    string GenerateToken(IEnumerable<Claim> claims, DateTime issuedAt, DateTime expiresAt);

    ClaimsPrincipal? ValidateToken(string token, out IDictionary<string, string>? extractedClaims);

    string HashToken(string token, out string salt);
}
