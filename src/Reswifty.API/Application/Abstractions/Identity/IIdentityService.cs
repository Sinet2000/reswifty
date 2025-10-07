namespace Reswifty.API.Application.Abstractions.Identity;

public interface IIdentityService
{
    string GetUserIdentity();

    string GetUserName();
}