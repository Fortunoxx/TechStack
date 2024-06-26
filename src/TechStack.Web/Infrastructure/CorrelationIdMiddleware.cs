namespace TechStack.Web.Infrastructure;

using Microsoft.Extensions.Primitives;
using TechStack.Application.Common.Interfaces;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;
    private const string _correlationIdHeader = "X-Correlation-Id";

    public async Task Invoke(HttpContext context, ICorrelationIdGenerator correlationIdGenerator)
    {
        var correlationId = GetCorrelationId(context, correlationIdGenerator);
        AddCorrelationIdHeaderToResponse(context, correlationId);

        await _next(context);
    }

    private static StringValues GetCorrelationId(HttpContext context, ICorrelationIdGenerator correlationIdGenerator)
    {
        if (context.Request.Headers.TryGetValue(_correlationIdHeader, out var correlationId))
        {
            correlationIdGenerator.Set(correlationId!);
            return correlationId;
        }
        else
        {
            return correlationIdGenerator.Get();
        }
    }

    private static void AddCorrelationIdHeaderToResponse(HttpContext context, StringValues correlationId)
    {
        context.Request.Headers.Append(_correlationIdHeader, new[] { correlationId.ToString() });
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Append(_correlationIdHeader, new[] { correlationId.ToString() });
            return Task.CompletedTask;
        });
    }
}