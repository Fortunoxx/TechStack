namespace TechStack.Infrastructure.Consumers;

using MassTransit;
using Microsoft.Extensions.Logging;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Test.Commands;

public class TestBusConsumer :
    IConsumer<TestCommand>,
    IConsumer<Fault<TestCommand>>
{
    private readonly ILockService lockService;
    private readonly ILogger<TestBusConsumer> logger;

    public TestBusConsumer(ILockService lockService, ILogger<TestBusConsumer> logger)
    {
        this.lockService = lockService;
        this.logger = logger;
    }

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