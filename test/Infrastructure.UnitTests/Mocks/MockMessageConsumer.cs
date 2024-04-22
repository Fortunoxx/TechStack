namespace Infrastructure.UnitTests.Mocks;

using MassTransit;
using TechStack.Application.Common.Models;

public class MockMessageConsumer : IConsumer<MockMessage>
{
    public async Task Consume(ConsumeContext<MockMessage> context)
    {
        await context.RespondAsync(new AcceptedResponse());
    }
}
