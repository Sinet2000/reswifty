namespace Reswifty.API.Application.Abstractions.Identity;

public interface ICurrentUserAccessor
{
    bool IsAuthenticated();

    string UserName();

    int UserId();

    string Email();
}
