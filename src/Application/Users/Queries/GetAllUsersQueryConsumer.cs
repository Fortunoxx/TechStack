namespace TechStack.Application.Users.Queries;

using AutoMapper;
using MassTransit;
using TechStack.Application.Common.Interfaces;

public class GetAllUsersQueryConsumer : IConsumer<GetAllUsersQuery>
{
    private readonly IApplicationDbContext applicationDbContext;
    private readonly IMapper mapper;

    public GetAllUsersQueryConsumer(IApplicationDbContext applicationDbContext, IMapper mapper)
    {
        this.applicationDbContext = applicationDbContext;
        this.mapper = mapper;
    }

    public async Task Consume(ConsumeContext<GetAllUsersQuery> context)
    {
        var items = mapper.Map<IEnumerable<GetUserByIdQueryResult>>(applicationDbContext.Users.AsAsyncEnumerable());
        await context.RespondAsync(new GetAllUsersQueryResult(items));
    }
}