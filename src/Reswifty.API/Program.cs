using System.Globalization;
using Dexlaris.EmailKit.Setup;
using Microsoft.AspNetCore.Localization;
using Reswifty.API.Application.Abstractions.Messaging;
using Reswifty.API.Infrastructure;
using Reswifty.API.Infrastructure.Email;
using Reswifty.API.Infrastructure.Extensions;
using Reswifty.API.Infrastructure.Logging;
using Reswifty.API.Options;
using Serilog;
using Serilog.Events;

namespace Reswifty.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        Serilog.Debugging.SelfLog.Enable(Console.Error);
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        var logger = Log.ForContext<Program>();

        logger.Information("Starting Application");

        try
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.ConfigureSerilog();

            builder.Services.ConfigureAppSettings(builder.Configuration);

            var apiConfig = builder.Configuration.GetSection(ApiConfig.SectionName).Get<ApiConfig>();
            ArgumentNullException.ThrowIfNull(apiConfig, nameof(apiConfig));
            var defaultCulture = new RequestCulture("en-US");
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = defaultCulture;

                var supportedCultures = apiConfig.SupportedCultures.Select(name => new CultureInfo(name)).ToArray();
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            builder.Services.ConfigureDatabase(builder.Configuration, builder.Environment);

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddMemoryCache();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddInfrastructure(builder.Configuration);

            MailSenderSetup.Configure(builder.Services);
            // builder.Services.AddScoped<IEmailService, EmailService>();
            // builder.Services.AddTransient<IEmailBgRunnerService, EmailDispatcher>();

            builder.AddServiceDefaults();
            builder.Services.AddProblemDetails();
            builder.AddDefaultOpenApi(builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0); // ← your default
                options.AssumeDefaultVersionWhenUnspecified = true; // ← magic line
                options.ReportApiVersions = true;
            }));
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application failed to start.");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}