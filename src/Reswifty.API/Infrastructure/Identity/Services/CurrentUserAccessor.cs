using System.Security;
using System.Security.Claims;
using System.Security.Principal;
using Reswifty.API.Application.Abstractions.Identity;
using Reswifty.ServiceDefaults;

namespace Reswifty.API.Infrastructure.Identity.Services;

public class CurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    private const string SystemUserName = "system";

    public bool IsAuthenticated()
    {
        var principal = User();

        if (principal is GenericPrincipal)
        {
            return false;
        }

        return principal.Identity?.IsAuthenticated == true;
    }

    public string UserName()
    {
        var principal = User();

        if (principal is GenericPrincipal)
        {
            return SystemUserName;
        }

        return principal.Identity?.IsAuthenticated == true
            ? principal.UserName() ?? string.Empty
            : SystemUserName;
    }

    public int UserId()
    {
        var principal = User();

        ValidateUserIsAuthenticated(principal);

        return principal.UserId();
    }

    public string Email()
    {
        var principal = User();
        ValidateUserIsAuthenticated(principal);

        return principal.UserEmail() ?? throw new Exception("User is not authenticated");
    }

    private static void ValidateUserIsAuthenticated(ClaimsPrincipal principal)
    {
        if (principal is GenericPrincipal || principal.Identity?.IsAuthenticated == false)
        {
            throw new SecurityException("User must be authenticated");
        }
    }

    private ClaimsPrincipal User()
    {
        return httpContextAccessor.HttpContext is not null
            ? httpContextAccessor.HttpContext.User
            : new GenericPrincipal(new GenericIdentity(SystemUserName), null);
    }
}
