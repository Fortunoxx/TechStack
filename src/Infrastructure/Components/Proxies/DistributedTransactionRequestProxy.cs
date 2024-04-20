using MassTransit;
using MassTransit.Courier;
using TechStack.Application.Common.Models;
using TechStack.Infrastructure.Components.Activities;

namespace TechStack.Infrastructure.Components.Proxies;

public class DistributedTransactionRequestProxy : RoutingSlipRequestProxy<DistributedTransactionCommand>
{
    protected override Task BuildRoutingSlip(RoutingSlipBuilder builder, ConsumeContext<DistributedTransactionCommand> request)
    {
        builder.SetVariables(
            new
            {
                request.Message.Key,
            }
        );

        builder.AddActivity(nameof(LogActivity), endpointNameFormatter<LogActivity, LogActivityArguments>());

        return Task.CompletedTask;
    }
}
