using System.Diagnostics;
using Reswifty.API.Options;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Reswifty.API.Infrastructure.Logging;

public static class LoggingSetup
{
    private const string ConsoleOutputTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj} {NewLine}{Exception}";

    private const string FileOutputTemplate =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj} " +
        "[ThreadId: {ThreadId}] [MachineName: {MachineName}]{NewLine}{Exception}";

    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        var loggingConfig = builder.Configuration.GetSection(LoggingConfig.SectionName).Get<LoggingConfig>();
        ArgumentNullException.ThrowIfNull(loggingConfig);

        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId();

            var env = context.HostingEnvironment;

            configuration.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                theme: AnsiConsoleTheme.Code,
                restrictedToMinimumLevel: LogEventLevel.Verbose);

            if (loggingConfig.Seq.Enabled && !string.IsNullOrWhiteSpace(loggingConfig.Seq.ApiKey))
            {
                configuration.WriteTo.Seq(
                    serverUrl: loggingConfig.Seq.Url,
                    apiKey: loggingConfig.Seq.ApiKey,
                    restrictedToMinimumLevel: LogEventLevel.Information);
            }

            if (builder.Environment.IsDevelopment() || Debugger.IsAttached)
            {
                configuration.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Code,
                    restrictedToMinimumLevel: LogEventLevel.Verbose);

                // Write warnings and above to a rolling file (errors-{Date}.txt)

                if (loggingConfig.IsWriteToFileEnabled)
                {
                    configuration.WriteTo.File(
                        path: "logs/errors-.txt",
                        rollingInterval: RollingInterval.Day,
                        restrictedToMinimumLevel: LogEventLevel.Warning,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}");
                }
            }
            else if (env.IsProduction())
            {
                // *** Database Sink (e.g. PostgreSQL) ***
                // if (loggingConfig.PgSql is not null && loggingConfig.PgSql.Enabled)
                // {
                //     if (!string.IsNullOrWhiteSpace(loggingConfig.PgSql.DbConnectionString))
                //     {
                //         var columnOptions = new Dictionary<string, ColumnWriterBase>
                //         {
                //             { "timestamp", new TimestampColumnWriter() },
                //             { "level", new LevelColumnWriter() },
                //             { "message", new RenderedMessageColumnWriter() },
                //             { "exception", new ExceptionColumnWriter() },
                //             { "properties", new LogEventSerializedColumnWriter() }
                //         };
                //
                //         configuration.WriteTo.Async(a => a.PostgreSQL(
                //             connectionString: loggingConfig.PgSql.DbConnectionString,
                //             tableName: "logs",
                //             needAutoCreateTable: true,
                //             columnOptions: columnOptions,
                //             restrictedToMinimumLevel: LogEventLevel.Error, // Only Error and Fatal
                //             batchSizeLimit: 50, // Post up to 50 events at a time
                //             period: TimeSpan.FromSeconds(5) // Every 5 seconds
                //         ));
                //     }
                //     else
                //     {
                //         Log.Information("Cannot configure DB logging as the connection string is empty.");
                //     }
                // }

                if (loggingConfig.IsWriteToFileEnabled)
                {
                    configuration.WriteTo.File(
                        path: "logs/critical-.txt",
                        rollingInterval: RollingInterval.Day,
                        restrictedToMinimumLevel: LogEventLevel.Fatal,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}");
                }

                // *** Grafana Loki Sink ***
                if (loggingConfig.Grafana is not null && loggingConfig.Grafana.Enabled)
                {
                    // configuration.WriteTo.GrafanaLoki(
                    //     endpointUrl: lokiUrl,
                    //     restrictedToMinimumLevel: LogEventLevel.Information);
                }
            }
            else
            {
                configuration.WriteTo.Console();
            }
        });
    }
}