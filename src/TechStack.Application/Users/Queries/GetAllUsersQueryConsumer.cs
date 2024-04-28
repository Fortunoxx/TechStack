namespace TechStack.Application.Users.Queries;

using AutoMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
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
        var items = await applicationDbContext.Users.ToListAsync();
        var mapped = mapper.Map<IEnumerable<GetUserByIdQueryResult>>(items);
        await context.RespondAsync(new GetAllUsersQueryResult(mapped));
    }
}