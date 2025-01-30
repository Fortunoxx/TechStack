namespace TechStack.Application.Users.Queries;

using MassTransit;
using TechStack.Application.Common.Interfaces;

public class FetchDataQueryConsumer(IAggregatorService aggregatorService) : IConsumer<FetchDataQuery>
{
    private readonly IAggregatorService aggregatorService = aggregatorService;

    public async Task Consume(ConsumeContext<FetchDataQuery> context)
    {
        var data = await aggregatorService.FetchAll();
        await context.RespondAsync(new FetchDataQueryResult(data));
    }
}
