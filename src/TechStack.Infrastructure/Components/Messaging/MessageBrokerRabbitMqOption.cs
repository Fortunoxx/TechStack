namespace TechStack.Infrastructure.Components.Messaging;

using MassTransit;
using Microsoft.Extensions.Configuration;

public abstract class MessageBrokerRabbitMqOption
{
    public abstract void Configure(
        IBusRegistrationContext context,
        IRabbitMqBusFactoryConfigurator busFactoryConfigurator,
        IConfiguration configuration);
}