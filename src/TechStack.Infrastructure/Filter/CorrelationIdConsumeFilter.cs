namespace TechStack.Infrastructure.Filter;

using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;

public class CorrelationIdConsumeFilter<T>(ILogger<CorrelationIdConsumeFilter<T>> logger) : IFilter<ConsumeContext<T>>
    where T : class
{
    private readonly ILogger<CorrelationIdConsumeFilter<T>> logger = logger;

    public void Probe(ProbeContext context) { }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var dictionary = new Dictionary<string, object>();

        if (context.Headers.TryGetHeader("X-Correlation-Id", out var correlationId))
        {
            dictionary["CorrelationId"] = correlationId;
        };

        using (logger.BeginScope(dictionary))
        {
            await next.Send(context);
        }
    }
}

