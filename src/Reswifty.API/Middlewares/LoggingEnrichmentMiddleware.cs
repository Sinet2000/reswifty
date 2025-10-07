using Serilog.Context;

namespace Reswifty.API.Middlewares;

public class LoggingEnrichmentMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        // grab existing X-Correlation-ID or make a new one
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();

        var remoteIpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var requestPath = context.Request.Path.HasValue
            ? context.Request.Path.Value!
            : "unknown";
        var httpMethod = context.Request.Method;

        // push into Serilog’s context so all logs for this request carry these props
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RemoteIpAddress", remoteIpAddress))
        using (LogContext.PushProperty("RequestPath", requestPath))
        using (LogContext.PushProperty("HttpMethod", httpMethod))
        {
            await next(context);
        }
    }
}