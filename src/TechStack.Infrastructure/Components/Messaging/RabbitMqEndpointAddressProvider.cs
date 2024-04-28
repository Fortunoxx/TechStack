namespace TechStack.Infrastructure.Components.Messaging;

using MassTransit;

public class RabbitMqEndpointAddressProvider(IEndpointNameFormatter formatter) : IEndpointAddressProvider
{
    private const string QueuePrefix = "queue:";
    private readonly IEndpointNameFormatter formatter = formatter;

    public Uri GetExecuteEndpoint<T, TArguments>()
        where T : class, IExecuteActivity<TArguments>
        where TArguments : class
        => new($"{QueuePrefix}{formatter.ExecuteActivity<T, TArguments>()}");
}