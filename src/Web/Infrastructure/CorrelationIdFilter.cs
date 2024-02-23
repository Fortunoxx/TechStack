namespace TechStack.Web.Infrastructure;

using Microsoft.AspNetCore.Mvc.Filters;
using TechStack.Application.Common.Interfaces;

public class CorrelationIdFilter : IActionFilter
{
    private readonly ICorrelationIdGenerator correlationIdGenerator;

    public CorrelationIdFilter(ICorrelationIdGenerator correlationIdGenerator)
        => this.correlationIdGenerator = correlationIdGenerator;

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var correlationId = context.HttpContext.Request.Headers["X-Correlation-Id"];
        if (!string.IsNullOrWhiteSpace(correlationId))
            correlationIdGenerator.Set(correlationId);
    }
}