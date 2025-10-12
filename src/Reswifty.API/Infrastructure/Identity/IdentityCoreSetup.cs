using System.Text;
using Dexlaris.Core.Exceptions;
using Dexlaris.Core.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Reswifty.API.Application.Abstractions.Identity;
using Reswifty.API.Application.Auth;
using Reswifty.API.Domain.Identity;
using Reswifty.API.Infrastructure.Identity.Services;
using Reswifty.API.Infrastructure.Persistence;
using Reswifty.API.Options;

namespace Reswifty.API.Infrastructure.Identity;

public static class IdentityCoreSetup
{
    public static IServiceCollection Configure<TContext>(
        IServiceCollection services, ConfigurationManager config, IEnumerable<string>? adminPermissions = null)
    {
        var authConfigSection = config.GetSection(AuthenticationOptions.SectionName);
        services.Configure<AuthenticationOptions>(authConfigSection);

        services.AddSingleton<IJwtTokenService>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
            var jwt = opts.Jwt ?? throw new InvalidOperationException("Jwt options missing");
            return new JwtTokenService(jwt.Secret, jwt.Issuer, jwt.Audience);
        });

        RegisterIdentity(services);

        var authConfig = authConfigSection.Get<AuthenticationOptions>();
        ContextualArgumentException.ThrowIfNull(authConfig);

        services.AddJwtAuthentication(authConfig.Jwt);

        services.AddAuthorizationBuilder()
            .AddPolicy("RequireAdminRole", policy =>
                policy.RequireRole("Admin"));

        return services
            .AddScoped<IIdentityService, IdentityService>()
            .AddScoped<IAuthService, AuthService>()
            .AddScoped<IIdentityUrlBuilder, IdentityUrlBuilder>()
            .AddSingleton<ICurrentUserAccessor, CurrentUserAccessor>();
    }

    private static IServiceCollection RegisterIdentity(this IServiceCollection services)
    {
        // DB is already added elsewhere:
        // services.AddDbContext<AppDbContext>(o => o.UseNpgsql(cs));

        services
            .AddIdentity<User, UserRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    private static void AddJwtAuthentication(this IServiceCollection services, JwtOptions config)
    {
        services
            .AddAuthentication(authentication =>
            {
                authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCookie(option => { option.LoginPath = config.AuthPage!.EnsureAffix("/"); })
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config.Issuer,
                    ValidAudience = config.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Secret)),
                };
            });
    }
}
