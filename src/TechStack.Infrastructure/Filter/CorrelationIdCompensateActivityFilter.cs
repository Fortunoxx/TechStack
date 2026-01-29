namespace TechStack.Infrastructure.Filter;

using MassTransit;
using Microsoft.Extensions.Logging;

/// <summary>
/// Filter that enriches logging scope with CorrelationId from routing slip variables
/// during activity compensation.
/// </summary>
public class CorrelationIdCompensateActivityFilter<TLog>(ILogger<CorrelationIdCompensateActivityFilter<TLog>> logger)
    : IFilter<CompensateContext<TLog>>
    where TLog : class
{
    private readonly ILogger<CorrelationIdCompensateActivityFilter<TLog>> logger = logger;

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("correlationIdCompensateActivity");
    }

    public async Task Send(CompensateContext<TLog> context, IPipe<CompensateContext<TLog>> next)
    {
        var dictionary = new Dictionary<string, object>();

        // Try to get CorrelationId from message headers
        // This value is added to the header in CorrelationIdPublishFilter
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
