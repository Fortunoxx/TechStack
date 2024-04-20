using MassTransit;
using MassTransit.Courier;
using MassTransit.Courier.Contracts;
using Microsoft.Extensions.Logging;
using TechStack.Application.Common.Models;

namespace TechStack.Application.Users.Commands;

public class ResponseProxy : RoutingSlipResponseProxy<DistributedTransactionCommand, DistributedTransactionResponse>
{
    protected override Task<DistributedTransactionResponse> CreateResponseMessage(ConsumeContext<RoutingSlipCompleted> context, DistributedTransactionCommand request)
    {
        var exceptions = context.Message.ActivityExceptions.Select(x => x.ExceptionInfo);
    }
}

public class DistributedTransactionCommandConsumer : IConsumer<DistributedTransactionCommand>
{
    private readonly ILogger<DistributedTransactionCommandConsumer> logger;

    public DistributedTransactionCommandConsumer(ILogger<DistributedTransactionCommandConsumer> logger)
        => this.logger = logger;

    public async Task Consume(ConsumeContext<DistributedTransactionCommand> context)
    {
        logger.LogDebug("Distributed Transaction Command received: {Key} {CorrelationId}", context.Message.Key, context.CorrelationId);

        var routingSlip = BuildRoutingSlip(context.Message);

        await context.Execute(routingSlip);

        if(context.ResponseAddress != null)
        {
            await context.RespondAsync<DistributedTransactionResponse>(new { Id = context.CorrelationId, });
        }
    }

    RoutingSlip BuildRoutingSlip(DistributedTransactionCommand command)
    {
        var builder = new RoutingSlipBuilder(NewId.NextGuid());
        
        // TODO: Add Variables
        // TODO: Add Activities

        return builder.Build();
    }
}
