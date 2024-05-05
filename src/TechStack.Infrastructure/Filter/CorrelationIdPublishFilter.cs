namespace TechStack.Infrastructure.Filter;

using System.Threading.Tasks;
using MassTransit;
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
