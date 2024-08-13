namespace TechStack.Infrastructure.Components.Consumers;

using MassTransit;
using Microsoft.Extensions.Logging;
using TechStack.Infrastructure.Contracts;

public class CreateTicketRequestConsumer(ILogger<ProcessRegistrationConsumer> logger) : IConsumer<CreateTicketRequest>
{
    private readonly ILogger<ProcessRegistrationConsumer> _logger = logger;

    public Task Consume(ConsumeContext<CreateTicketRequest> context)
    {
        _logger.LogWarning("Creating ticket from {@Message}", context.Message);

        // throw new InvalidOperationException("Something bad happened here");

        return Task.CompletedTask;
    }
}
