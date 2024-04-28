namespace TechStack.Application.DistributedTransaction.Commands;

using MassTransit;
using MassTransit.Courier.Contracts;
using Microsoft.Extensions.Logging;
using TechStack.Application.Common.Models;

public class DistributedTransactionCommandConsumer : IConsumer<DistributedTransactionCommand>
{
    private readonly ILogger<DistributedTransactionCommandConsumer> logger;

    public DistributedTransactionCommandConsumer(ILogger<DistributedTransactionCommandConsumer> logger)
        => this.logger = logger;

    public async Task Consume(ConsumeContext<DistributedTransactionCommand> context)
    {
        logger.LogWarning("Distributed Transaction Command received: {Key} {CorrelationId}", context.Message.Key, context.CorrelationId);

        var routingSlip = BuildRoutingSlip(context.Message);

        await context.Execute(routingSlip);

        if (context.ResponseAddress != null)
        {
            await context.RespondAsync(new DistributedTransactionResponse(NewId.NextGuid()));
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


