namespace TechStack.Infrastructure.Consumers;

using MassTransit;
using Microsoft.Extensions.Logging;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Common.Models;

public class TestQueryConsumer : IConsumer<TestQuery>
{
    private readonly ILockService lockService;
    private readonly ILogger<TestQueryConsumer> logger;

    public TestQueryConsumer(ILockService lockService, ILogger<TestQueryConsumer> logger)
    {
        this.lockService = lockService;
        this.logger = logger;
    }

    public async Task Consume(ConsumeContext<TestQuery> context)
    {
        var result = new TestResult(context.Message.Id, $"Hello from {nameof(TestQueryConsumer)}");

        lockService.CreateLock(context.Message.Id);

        await context.RespondAsync(result);
    }
}