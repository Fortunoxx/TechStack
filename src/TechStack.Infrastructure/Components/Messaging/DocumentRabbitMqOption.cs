namespace TechStack.Infrastructure.Components.Messaging;

using MassTransit;
using Microsoft.Extensions.Configuration;
using TechStack.Infrastructure.Components.Proxies;

public class DocumentRabbitMqOption : MessageBrokerRabbitMqOption
{
    public override void Configure(
        IBusRegistrationContext context,
        IRabbitMqBusFactoryConfigurator busFactoryConfigurator,
        IConfiguration configuration)
    {
        busFactoryConfigurator.ReceiveEndpoint(
            context.EndpointNameFormatter.Consumer<DistributedTransactionRequestProxy>(),
            e =>
            {
                var routingSlipProxy = new DistributedTransactionRequestProxy(context.EndpointNameFormatter);
                var routingSlipResponseProxy = new DistributedTransactionResponseProxy();
                e.Instance(routingSlipProxy);
                e.Instance(routingSlipResponseProxy);
                e.UseMessageRetry(r => r.Interval(5, 420));
            });
    }
}
