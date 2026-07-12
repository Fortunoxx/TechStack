namespace TechStack.Application.Users.Queries;

using MassTransit;
using Microsoft.EntityFrameworkCore;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Common.Mappings;

public class GetAllUsersQueryConsumer : IConsumer<GetAllUsersQuery>
{
    private readonly IApplicationDbContext applicationDbContext;
    private readonly UserMapper mapper;

    public GetAllUsersQueryConsumer(IApplicationDbContext applicationDbContext, UserMapper mapper)
    {
        this.applicationDbContext = applicationDbContext;
        this.mapper = mapper;
    }

    public async Task Consume(ConsumeContext<GetAllUsersQuery> context)
    {
        var items = await applicationDbContext.Users.Include(x => x.MetaData).ToListAsync();
        var mapped = items.Select(mapper.UserToGetUserByIdQueryResult);
        await context.RespondAsync(new GetAllUsersQueryResult(mapped));
    }
}