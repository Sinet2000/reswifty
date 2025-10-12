namespace Reswifty.API.Common.Exceptions.Auth;

public sealed class InvalidRefreshTokenException : Exception
{
    public InvalidRefreshTokenException() : base("invalid_refresh_token") { }
}

public sealed class RefreshTokenExpiredException : Exception
{
    public RefreshTokenExpiredException() : base("refresh_token_expired") { }
}

public sealed class RefreshTokenRevokedException : Exception
{
    public RefreshTokenRevokedException() : base("refresh_token_revoked") { }
}

public sealed class AccountUnavailableException : Exception
{
    public AccountUnavailableException() : base("account_unavailable") { }
}
