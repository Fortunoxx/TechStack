namespace TechStack.Infrastructure.Filter;

using System.Threading.Tasks;
using MassTransit;
using TechStack.Application.Common.Interfaces;

public class CorrelationIdConsumeFilter<T> : IFilter<ConsumeContext<T>>
    where T : class
{
    public void Probe(ProbeContext context) { }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        if (typeof(ICorrelationId).IsAssignableFrom(typeof(T)))
        {
            // context.CorrelationId = correlationId;
        }

        await next.Send(context);
    }
}