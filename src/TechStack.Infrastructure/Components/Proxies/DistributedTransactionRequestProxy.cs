namespace TechStack.Infrastructure.Components.Proxies;

using MassTransit;
using MassTransit.Courier;
using Microsoft.Extensions.Logging;
using TechStack.Application.Common.Models;
using TechStack.Infrastructure.Components.Activities;
using TechStack.Infrastructure.Components.Messaging;

public class DistributedTransactionRequestProxy(IEndpointAddressProvider endpointAddressProvider) : RoutingSlipRequestProxy<DistributedTransactionCommand>
{
    protected override Task BuildRoutingSlip(RoutingSlipBuilder builder, ConsumeContext<DistributedTransactionCommand> request)
    {
        builder.SetVariables(
            new
            {
                request.Message.Key,
            }
        );

        builder.AddActivity(nameof(LogActivity), endpointAddressProvider.GetExecuteEndpoint<LogActivity, LogActivityArguments>()
            , new { LogLevel = LogLevel.Information, Message = $"Routing Slip starting ({request.Message.Key})", });

        return Task.CompletedTask;
    }
}

