namespace TechStack.Application.Users.Queries;

using System.Net;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Common.Mappings;
using TechStack.Application.Common.Models;

public class GetUserByIdQueryConsumer : IConsumer<GetUserByIdQuery>
{
    private readonly IApplicationDbContext applicationDbContext;
    private readonly UserMapper mapper;

    public GetUserByIdQueryConsumer(IApplicationDbContext applicationDbContext, UserMapper mapper)
    {
        this.applicationDbContext = applicationDbContext;
        this.mapper = mapper;
    }

    public async Task Consume(ConsumeContext<GetUserByIdQuery> context)
    {
        var user = await applicationDbContext.Users.AsNoTracking().SingleAsync(x => x.Id == context.Message.Id);

        if (user == null)
        {
            await context.RespondAsync(new FaultedResponse(HttpStatusCode.NotFound, new { Message = "User not found", }));
            return;
        }

        var result = mapper.UserToGetUserByIdQueryResult(user);
        await context.RespondAsync(result);
    }
}