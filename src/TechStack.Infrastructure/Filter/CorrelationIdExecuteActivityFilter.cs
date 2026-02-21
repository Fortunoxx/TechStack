namespace TechStack.Infrastructure.Filter;

using MassTransit;
using Microsoft.Extensions.Logging;

/// <summary>
/// Filter that enriches logging scope with CorrelationId from routing slip variables
/// during activity execution.
/// </summary>
public class CorrelationIdExecuteActivityFilter<TArguments>(ILogger<CorrelationIdExecuteActivityFilter<TArguments>> logger)
    : IFilter<ExecuteContext<TArguments>>
    where TArguments : class
{
    private readonly ILogger<CorrelationIdExecuteActivityFilter<TArguments>> logger = logger;

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("correlationIdExecuteActivity");
    }

    public async Task Send(ExecuteContext<TArguments> context, IPipe<ExecuteContext<TArguments>> next)
    {
        var dictionary = new Dictionary<string, object>();

        // Try to get CorrelationId from message headers
        if (context.Headers.TryGetHeader("X-Correlation-Id", out var correlationIdHeaderValue)
            && correlationIdHeaderValue is string correlationIdString
            && !string.IsNullOrWhiteSpace(correlationIdString))
        {
            dictionary["CorrelationId"] = correlationIdString;
        }

        // Also capture TrackingNumber as a fallback identifier
        dictionary["TrackingNumber"] = context.TrackingNumber;

        // Optionally add ActivityName for better context
        dictionary["ActivityName"] = context.ActivityName;

        using (logger.BeginScope(dictionary))
        {
            await next.Send(context);
        }
    }
}
