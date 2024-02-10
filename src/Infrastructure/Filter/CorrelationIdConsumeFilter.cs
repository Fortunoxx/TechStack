namespace TechStack.Infrastructure.Filter;

using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using TechStack.Application.Common.Interfaces;

public class CorrelationIdConsumeFilter<T> : IFilter<ConsumeContext<T>>
    where T : class
{
    private readonly ILogger<CorrelationIdConsumeFilter<T>> logger;
    private readonly ICorrelationIdGenerator correlationIdGenerator;

    public CorrelationIdConsumeFilter(ILogger<CorrelationIdConsumeFilter<T>> logger, ICorrelationIdGenerator correlationIdGenerator)
    {
        this.logger = logger;
        this.correlationIdGenerator = correlationIdGenerator;
    }

    public void Probe(ProbeContext context) { }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var dictionary = new Dictionary<string, object> { ["CorrelationId"] = correlationIdGenerator.Get(), };

        using (logger.BeginScope(dictionary))
        {

            await next.Send(context);
        }
    }
}