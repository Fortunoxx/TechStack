namespace TechStack.Infrastructure.Components.Proxies;

using MassTransit;
using MassTransit.Courier;
using TechStack.Application.Common.Models;
using TechStack.Infrastructure.Components.Activities;
using TechStack.Infrastructure.Components.Messaging;

public class DistributedTransactionRequestProxy(IEndpointNameFormatter endpointNameFormatter) : RoutingSlipRequestProxy<DistributedTransactionCommand>
{
    private readonly RabbitMqEndpointAddressProvider endpointAddressProvider = new(endpointNameFormatter);

    protected override Task BuildRoutingSlip(RoutingSlipBuilder builder, ConsumeContext<DistributedTransactionCommand> request)
    {
        builder.SetVariables(
            new
            {
                request.Message.Key,
            }
        );

        builder.AddActivity(nameof(LogActivity), endpointAddressProvider.GetExecuteEndpoint<LogActivity, LogActivityArguments>());

        return Task.CompletedTask;
    }
}

