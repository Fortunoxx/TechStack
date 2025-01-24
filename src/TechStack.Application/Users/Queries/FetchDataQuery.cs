using MassTransit;
using TechStack.Application.Common.Interfaces;

namespace TechStack.Application.Users.Queries;

public record FetchDataQuery();

public class FetchDataQueryConsumer(IAggregatorService aggregatorService) : IConsumer<FetchDataQuery>
{
    private IAggregatorService aggregatorService = aggregatorService;

    public async Task Consume(ConsumeContext<FetchDataQuery> context)
    {
        var data = await aggregatorService.FetchAll();
        await context.RespondAsync(new FetchDataQueryResult(data));
    }
}

public record FetchDataQueryResult(IDictionary<string, string> Data);