using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace Reswifty.ServiceDefaults;

public static partial class Extensions
{
    public static IApplicationBuilder UseDefaultOpenApi(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        var configuration = app.Configuration;
        var openApiSection = configuration.GetSection("OpenApi");

        if (!openApiSection.Exists())
        {
            return app;
        }

        app.MapOpenApi();

        app.MapScalarApiReference(options =>
        {
            // Disable default fonts to avoid download unnecessary fonts
            options.DefaultFonts = false;
        });
        app.MapGet("/api-docs", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

        return app;
    }

    public static IHostApplicationBuilder AddDefaultOpenApi(this IHostApplicationBuilder builder)
    {
        var openApi = builder.Configuration.GetSection("OpenApi");
        var identitySection = builder.Configuration.GetSection("Identity");

        var scopes = identitySection.Exists()
            ? identitySection.GetRequiredSection("Scopes").GetChildren().ToDictionary(p => p.Key, p => p.Value)
            : new Dictionary<string, string?>();

        if (!openApi.Exists())
        {
            return builder;
        }

        builder.Services.AddOpenApi();

        return builder;
    }
}