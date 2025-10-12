using System.Security.Claims;
using System.Security;
using Reswifty.API.Application.Abstractions.Identity;
using Reswifty.ServiceDefaults;

namespace Reswifty.API.Infrastructure.Identity.Services;

public sealed class CurrentUserAccessor(IHttpContextAccessor http) : ICurrentUserAccessor
{
    private static readonly ClaimsPrincipal SystemPrincipal =
        new(new ClaimsIdentity([new Claim(ClaimTypes.Name, SystemUserName)], authenticationType: null));

    private const string SystemUserName = "system";

    public ClaimsPrincipal Principal =>
        http.HttpContext?.User ?? SystemPrincipal;

    public bool IsAuthenticated =>
        Principal.Identity?.IsAuthenticated ?? false;

    public bool IsSystem =>
        !IsAuthenticated;

    public string? TryGetUserName()
    {
        if (!IsAuthenticated) return SystemUserName;

        // Prefer Name, then preferred_username, then Email
        return Principal.FindFirstValue(ClaimTypes.Name)
               ?? Principal.FindFirstValue("preferred_username")
               ?? Principal.FindFirstValue(ClaimTypes.Email)
               ?? SystemUserName;
    }

    public string GetRequiredUserName()
        => TryGetUserName() ?? SystemUserName;

    public int? TryGetUserId()
    {
        if (!IsAuthenticated) return null;

        var raw = Principal.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? Principal.FindFirstValue("sub"); // some IdPs only set 'sub'

        return int.TryParse(raw, out var id) ? id : null;
    }

    public int GetRequiredUserId()
        => TryGetUserId() ?? throw new SecurityException("Authenticated user id is missing or invalid.");

    public string? TryGetEmail()
    {
        if (!IsAuthenticated) return null;
        return Principal.FindFirstValue(ClaimTypes.Email);
    }

    public string GetRequiredEmail()
        => TryGetEmail() ?? throw new SecurityException("Authenticated user email is missing.");
}
