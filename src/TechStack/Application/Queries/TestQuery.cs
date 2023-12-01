namespace TechStack.Application.Queries;

using MassTransit;

public record TestQuery(int Id);

public record TestResult(int Id, string Text);

public class TestQueryConsumer : IConsumer<TestQuery>
{
    public async Task Consume(ConsumeContext<TestQuery> context)
    {
        var result = new TestResult(context.Message.Id, $"Hello from {nameof(TestQueryConsumer)}");
        await context.RespondAsync(result);
    }
}