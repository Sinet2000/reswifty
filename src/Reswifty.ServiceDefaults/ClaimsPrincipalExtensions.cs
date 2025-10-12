using System.Security.Claims;

namespace Reswifty.ServiceDefaults;

public static class ClaimsPrincipalExtensions
{
    public static int UserId(this ClaimsPrincipal principal)
    {
        var userIdClaimValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(userIdClaimValue, out var userId) ? userId : throw new Exception("Invalid user id");
    }

    public static string? UserName(this ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.Name)?.Value;

    public static string? UserEmail(this ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.Email)?.Value;
}
