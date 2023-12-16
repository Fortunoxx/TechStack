namespace TechStack.Infrastructure.Consumers;

using MassTransit;
using Microsoft.Extensions.Logging;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Common.Models;

public class TestConsumer : 
    IConsumer<TestQuery>, 
    IConsumer<TestCommand>, 
    IConsumer<Fault<TestCommand>>
{
    private readonly ILockService lockService;
    private readonly ILogger<TestConsumer> logger;

    public TestConsumer(ILockService lockService, ILogger<TestConsumer> logger)
    {
        this.lockService = lockService;
        this.logger = logger;
    }

    // Mediator
    public async Task Consume(ConsumeContext<TestQuery> context)
    {
        var result = new TestQueryResult(context.Message.Id, $"Hello from {nameof(TestConsumer)}");
        await context.RespondAsync(result);
    }

    // Bus
    public async Task Consume(ConsumeContext<TestCommand> context)
    {
        lockService.CreateLock(context.Message.Id);
        logger.LogWarning("Lock created: {id}", context.Message.Id);
        await context.RespondAsync(new TestCommandResponse(true));
    }

    // Why is there no Fault<TestCommand> message, that can be consumed here?
    public async Task Consume(ConsumeContext<Fault<TestCommand>> context)
    {
        logger.LogError("Encountered error {message}", context.Message);
        await context.RespondAsync(new TestCommandResponse(false));
    }
}