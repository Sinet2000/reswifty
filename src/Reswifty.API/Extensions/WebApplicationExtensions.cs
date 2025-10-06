using Asp.Versioning;
using Scalar.AspNetCore;

namespace CSNgAssist.API.Extensions;

public static class WebApplicationExtensions
{
    public static IApplicationBuilder UseDefaultOpenApi(this WebApplication app)
    {
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
        app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
        
        // TODO ("REturn back")
        // if (app.Environment.IsDevelopment())
        // {
        //     app.MapScalarApiReference(options =>
        //     {
        //         // Disable default fonts to avoid download unnecessary fonts
        //         options.DefaultFonts = false;
        //     });
        //     app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
        // }

        return app;
    }

    public static IHostApplicationBuilder AddDefaultOpenApi(
        this IHostApplicationBuilder builder,
        IApiVersioningBuilder? apiVersioning = null)
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

        if (apiVersioning is not null)
        {
            // the default format will just be ApiVersion.ToString(); for example, 1.0.
            // this will format the version as "'v'major[.minor][-status]"
            var versioned = apiVersioning.AddApiExplorer(options => options.GroupNameFormat = "'v'VVV");
            string[] versions = ["v1", "v2"];
            foreach (var description in versions)
            {
                builder.Services.AddOpenApi(description, options =>
                {
                    options.ApplyApiVersionInfo(
                        openApi["Document:Title"] ??
                        throw new InvalidOperationException($"Configuration missing value for: {openApi.Path + ":Document:Title"}"),
                        openApi["Document:Description"] ?? throw new InvalidOperationException(
                            $"Configuration missing value for: {openApi.Path + ":Document:Description"}"));

                    options.ApplyAuthorizationChecks([.. scopes.Keys]);
                    options.ApplySecuritySchemeDefinitions();
                    options.ApplyOperationDeprecatedStatus();
                    options.ApplyApiVersionDescription();
                    options.ApplySchemaNullableFalse();
                    // Clear out the default servers so we can fallback to
                    // whatever ports have been allocated for the service by Aspire
                    options.AddDocumentTransformer((document, context, cancellationToken) =>
                    {
                        document.Servers = [];

                        return Task.CompletedTask;
                    });
                });
            }
        }

        return builder;
    }
}