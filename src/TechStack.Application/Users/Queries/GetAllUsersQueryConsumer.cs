namespace TechStack.Application.Users.Queries;

using Mapster;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using TechStack.Application.Common.Interfaces;

public class GetAllUsersQueryConsumer : IConsumer<GetAllUsersQuery>
{
    private readonly IApplicationDbContext applicationDbContext;

    public GetAllUsersQueryConsumer(IApplicationDbContext applicationDbContext)
    {
        this.applicationDbContext = applicationDbContext;
    }

    public async Task Consume(ConsumeContext<GetAllUsersQuery> context)
    {
        var items = await applicationDbContext.Users.Include(x => x.MetaData).ToListAsync();
        var mapped = items.Adapt<IEnumerable<GetUserByIdQueryResult>>();
        await context.RespondAsync(new GetAllUsersQueryResult(mapped));
    }
}