namespace TechStack.Infrastructure.Filter;

using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using TechStack.Application.Common.Interfaces;

public class CorrelationIdPublishFilter<T>(ICorrelationIdGenerator correlationIdGenerator) : IFilter<PublishContext<T>>
    where T : class
{
    private readonly ICorrelationIdGenerator correlationIdGenerator = correlationIdGenerator;

    public Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        context.Headers.Set("X-Correlation-Id", correlationIdGenerator.Get());

        return next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
    }
}

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