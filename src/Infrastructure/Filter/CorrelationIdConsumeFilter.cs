namespace TechStack.Infrastructure.Filter;

using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;

public class CorrelationIdConsumeFilter<T> : IFilter<ConsumeContext<T>>
    where T : class
{
    private readonly ILogger<CorrelationIdConsumeFilter<T>> logger;
    
    public CorrelationIdConsumeFilter(ILogger<CorrelationIdConsumeFilter<T>> logger)
        => this.logger = logger;

    public void Probe(ProbeContext context) { }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var dictionary = new Dictionary<string, object> { ["CorrelationId"] = context.CorrelationId, };

        using (logger.BeginScope(dictionary))
        {
            await next.Send(context);
        }
    }
}