using MassTransit;
using MassTransit.Courier;
using MassTransit.Courier.Contracts;
using TechStack.Application.Common.Models;

namespace TechStack.Infrastructure.Components.Proxies;

public class DistributedTransactionResponseProxy :
    RoutingSlipResponseProxy<DistributedTransactionCommand, DistributedTransactionResponse>
{
    protected override Task<DistributedTransactionResponse> CreateResponseMessage(ConsumeContext<RoutingSlipCompleted> context, DistributedTransactionCommand request)
    {
        // in case we need something from the response, we can access it from the context here
        context.GetVariable<string>("Key");
        return Task.FromResult(new DistributedTransactionResponse(NewId.NextGuid()));
    }
}
