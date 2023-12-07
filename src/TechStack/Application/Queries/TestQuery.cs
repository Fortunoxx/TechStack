namespace TechStack.Application.Queries;

using MassTransit;

public record TestQuery(int Id);

public record TestResult(int Id, string Text);

public class TestQueryConsumer : IConsumer<TestQuery>
{
    private readonly ILogger<TestQueryConsumer> logger;

    public TestQueryConsumer(ILogger<TestQueryConsumer> logger)
        => this.logger = logger;

    public async Task Consume(ConsumeContext<TestQuery> context)
    {
        var result = new TestResult(context.Message.Id, $"Hello from {nameof(TestQueryConsumer)}");
        if (context.Message.Id > 100)
        {
            logger.LogError("Intentionally throw exception for {id}", context.Message.Id);
            throw new NotFiniteNumberException("Provoked exception to trigger retry", context.Message.Id);
        }
        await context.RespondAsync(result);
    }
}