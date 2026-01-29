namespace TechStack.Application.Test.Queries;

using MassTransit;

public class TestQueryMediatorConsumer :
    IConsumer<TestQuery>
{
    public async Task Consume(ConsumeContext<TestQuery> context)
    {
        var result = new TestQueryResult(context.Message.Id, $"Hello from {nameof(TestQueryMediatorConsumer)}");
        await context.RespondAsync(result);
    }
}
