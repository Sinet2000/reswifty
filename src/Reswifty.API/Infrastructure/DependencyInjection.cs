using System.Text;
using Dexlaris.Core.Exceptions;
using Dexlaris.Core.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Reswifty.API.Application.Abstractions.Identity;
using Reswifty.API.Application.Abstractions.Persistence;
using Reswifty.API.Application.Abstractions.Repositories;
using Reswifty.API.Application.Companies.Services;
using Reswifty.API.Domain.Identity;
using Reswifty.API.Infrastructure.Extensions;
using Reswifty.API.Infrastructure.Identity;
using Reswifty.API.Infrastructure.Identity.Services;
using Reswifty.API.Infrastructure.Persistence;
using Reswifty.API.Infrastructure.Persistence.Repositories;
using Reswifty.API.Infrastructure.Persistence.Seed;
using Reswifty.API.Options;
using Reswifty.ServiceDefaults;

namespace Reswifty.API.Infrastructure;

public static class DependencyInjection
{
    public static void ConfigureAppSettings(this IServiceCollection services)
    {
        services.AddOptions<FileStorageOptions>().BindConfiguration(nameof(FileStorageOptions.SectionName));
        services.AddOptions<ApiOptions>().BindConfiguration(nameof(ApiOptions.SectionName));
    }

    public static void AddPersistence(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("AppDb");
        ContextualArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        builder.Services.AddDbContext<AppDbContext>(options => { options.UseNpgsql(connectionString); });
        builder.EnrichNpgsqlDbContext<AppDbContext>();

        builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        builder.Services.AddMigration<AppDbContext, AppDbContextSeed>();
    }

    public static void AddApplication(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // MediatR: scan this assembly (commands/queries/handlers live here)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        // Validators (optional but common)
        // services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Add the authentication services to DI
        builder.AddDefaultAuthentication();

        services.AddHttpContextAccessor();
        services.AddTransient<IIdentityService, IdentityService>();

        services.AddScoped<ICompanyService, CompanyService>();

        // Pipeline behaviors (optional)
        // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // e.g., email, caching, blob storage, etc.
        return services;
    }
}
