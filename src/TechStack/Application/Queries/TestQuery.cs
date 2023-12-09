namespace TechStack.Application.Queries;

using MassTransit;
using TechStack.Services;

public record TestQuery(int Id);

public record TestResult(int Id, string Text);

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

        lockService.AquireLock(context.Message.Id);

        await context.RespondAsync(result);
    }
}