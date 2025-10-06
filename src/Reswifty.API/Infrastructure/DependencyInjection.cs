using Dexlaris.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using Reswifty.API.Application.Abstractions.Persistence;
using Reswifty.API.Application.Abstractions.Repositories;
using Reswifty.API.Application.Companies.Services;
using Reswifty.API.Infrastructure.Persistence;
using Reswifty.API.Infrastructure.Persistence.Repositories;
using Reswifty.API.Options;

namespace Reswifty.API.Infrastructure;

public static class DependencyInjection
{
    public static void ConfigureAppSettings(this IServiceCollection service, IConfiguration configuration)
    {
        service.Configure<FileStorageConfig>(configuration.GetSection(FileStorageConfig.SectionName));
        service.Configure<ApiConfig>(configuration.GetSection(ApiConfig.SectionName));
    }

    public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        var connectionString = configuration.GetConnectionString("AppDb");
        ContextualArgumentException.ThrowIfNull(connectionString);

        services.AddDbContext<AppDbContext>(o =>
            o.UseNpgsql(cs, npg => npg.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        // Repos & UoW
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // App Services
        services.AddScoped<ICompanyService, CompanyService>();

        return services;
    }
}