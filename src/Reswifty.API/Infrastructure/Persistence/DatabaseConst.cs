namespace Reswifty.API.Infrastructure.Persistence;

public static class DatabaseConst
{
    public static class Schemas
    {
        public const string Default = "public";
        public const string Communication = "cmnct";
        public const string User = "usr";
    }

    public static class Tables
    {
        public const string User = "users";
        public const string Role = "roles";
        public const string UserRole = "user_roles";
        public const string RoleClaim = "role_claims";
        public const string UserClaim = "user_claims";
        public const string UserLogin = "user_logins";
        public const string UserToken = "user_tokens";
        public const string UserInvite = "user_invites";
        public const string RefreshToken = "refresh_tokens";

        public const string Files = "files";
        public const string Translations = "translations";
    }
}
