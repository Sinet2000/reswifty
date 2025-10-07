using Reswifty.API.Application.Abstractions.Identity;

namespace Reswifty.API.Infrastructure.Identity;

public class IdentityService(IHttpContextAccessor context) : IIdentityService
{
    public string GetUserIdentity()
        => context.HttpContext?.User.FindFirst("sub")?.Value ?? string.Empty;

    public string GetUserName()
        => context.HttpContext?.User.Identity?.Name ?? string.Empty;
}