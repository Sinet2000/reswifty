using System.Security.Claims;

namespace Reswifty.API.Application.Abstractions.Identity;

public interface ICurrentUserAccessor
{
    bool IsAuthenticated { get; }
    bool IsSystem { get; }

    string? TryGetUserName();
    string GetRequiredUserName();

    int? TryGetUserId();
    int GetRequiredUserId();

    string? TryGetEmail();
    string GetRequiredEmail();

    ClaimsPrincipal Principal { get; }
}
