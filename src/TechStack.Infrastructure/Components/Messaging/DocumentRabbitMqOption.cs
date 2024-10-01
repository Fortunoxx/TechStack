namespace TechStack.Infrastructure.Components.Messaging;

using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechStack.Infrastructure.Components.Proxies;

public class DocumentRabbitMqOption(IServiceCollection services) : MessageBrokerRabbitMqOption
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
                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var endpointAddressProvider = scopedServices.GetRequiredService<IEndpointAddressProvider>();

                var routingSlipProxy = new DistributedTransactionRequestProxy(endpointAddressProvider);
                var routingSlipResponseProxy = new DistributedTransactionResponseProxy();
                e.Instance(routingSlipProxy);
                e.Instance(routingSlipResponseProxy);
                e.UseMessageRetry(r => r.Interval(5, 420));
            });
    }
}
