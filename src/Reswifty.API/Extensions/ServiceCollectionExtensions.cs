using Microsoft.EntityFrameworkCore;
using Reswifty.API.Application.Abstractions.Identity;
using Reswifty.API.Application.Companies.Services;
using Reswifty.API.Infrastructure.Identity;
using Reswifty.API.Infrastructure.Persistence;
using Reswifty.ServiceDefaults;

namespace Reswifty.API.Extensions;

internal static class ServiceCollectionExtensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // Add the authentication services to DI
        builder.AddDefaultAuthentication();

        services.AddDbContext<AppDbContext>(options => { options.UseNpgsql(builder.Configuration.GetConnectionString("AppDb")); });
        builder.EnrichNpgsqlDbContext<AppDbContext>();

        // services.AddMigration<AppDbContext, AppDbContextSeed>();

        services.AddHttpContextAccessor();
        services.AddTransient<IIdentityService, IdentityService>();

        // Configure MediatR

        services.AddScoped<ICompanyService, CompanyService>();
    }
}