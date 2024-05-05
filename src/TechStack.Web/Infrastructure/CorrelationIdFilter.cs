namespace TechStack.Web.Infrastructure;

using Microsoft.AspNetCore.Mvc.Filters;
using TechStack.Application.Common.Interfaces;

public class CorrelationIdFilter(ICorrelationIdGenerator correlationIdGenerator) : IActionFilter
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    private readonly ICorrelationIdGenerator correlationIdGenerator = correlationIdGenerator;

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
        {
            correlationIdGenerator.Set(correlationId!);
        }
        else
        {
            context.HttpContext.Request.Headers.Append(CorrelationIdHeader, correlationIdGenerator.Get());
        }
    }
}
