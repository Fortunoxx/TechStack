namespace TechStack.Web.Infrastructure;

using Serilog.Events;

public static class LogHelper
{
    public static LogEventLevel CustomGetLevel(HttpContext ctx, double _, Exception? ex) =>
        ex != null
            ? LogEventLevel.Error 
            : ctx.Response.StatusCode > 499 
                ? LogEventLevel.Error 
                : IsHealthCheckEndpoint(ctx) || IsMetricsEndpoint(ctx) // Not an error, check if it was a health check
                    ? LogEventLevel.Verbose // Was a health check, use Verbose
                    : LogEventLevel.Information;

    private static bool IsMetricsEndpoint(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint is object) // same as !(endpoint is null)
        {
            return string.Equals(
                endpoint.DisplayName,
                "/metrics",
                StringComparison.Ordinal);
        }
        // No endpoint, so not a metrics endpoint
        return false;
    }
    
    private static bool IsHealthCheckEndpoint(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint is object) // same as !(endpoint is null)
        {
            return string.Equals(
                endpoint.DisplayName,
                "Health checks",
                StringComparison.Ordinal);
        }
        // No endpoint, so not a health check endpoint
        return false;
    }

    private static LogEventLevel DetermineByPath(HttpRequest httpRequest)
    {
        if (httpRequest?.Path.Value?.EndsWith("/health") == true)
        {
            return LogEventLevel.Verbose;
        }
        else if (httpRequest?.Path.Value?.EndsWith("/metrics") == true)
        {
            return LogEventLevel.Verbose;
        }

        return LogEventLevel.Information;
    }
}
