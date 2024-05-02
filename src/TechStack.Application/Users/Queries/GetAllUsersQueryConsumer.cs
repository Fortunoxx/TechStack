namespace TechStack.Application.Users.Queries;

using AutoMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using TechStack.Application.Common.Interfaces;

public class GetAllUsersQueryConsumer(IApplicationDbContext applicationDbContext, IMapper mapper) 
    : IConsumer<GetAllUsersQuery>
{
    private readonly IApplicationDbContext applicationDbContext = applicationDbContext;
    
    private readonly IMapper mapper = mapper;

    public async Task Consume(ConsumeContext<GetAllUsersQuery> context)
    {
        var items = await applicationDbContext.Users.ToListAsync();
        var mapped = mapper.Map<IEnumerable<GetUserByIdQueryResult>>(items);
        await context.RespondAsync(new GetAllUsersQueryResult(mapped));
    }
}