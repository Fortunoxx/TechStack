namespace TechStack.Infrastructure.Consumers;

using MassTransit;
using TechStack.Application.Common.Models;

public class TestMediatorConsumer :
    IConsumer<TestQuery>
{
    public async Task Consume(ConsumeContext<TestQuery> context)
    {
        var result = new TestQueryResult(context.Message.Id, $"Hello from {nameof(TestMediatorConsumer)}");
        await context.RespondAsync(result);
    }
}
