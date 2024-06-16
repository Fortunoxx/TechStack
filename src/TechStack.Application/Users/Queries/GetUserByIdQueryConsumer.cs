namespace TechStack.Application.Users.Queries;

using System.Diagnostics;
using System.Net;
using AutoMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Common.Models;

public class GetUserByIdQueryConsumer(IApplicationDbContext applicationDbContext, IMapper mapper) 
    : IConsumer<GetUserByIdQuery>
{
    private readonly IApplicationDbContext applicationDbContext = applicationDbContext;
    private readonly IMapper mapper = mapper;

    public async Task Consume(ConsumeContext<GetUserByIdQuery> context)
    {
        Activity.Current?.AddEvent(new ActivityEvent("Getting Users from DB"));
        var user = await applicationDbContext.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == context.Message.Id);
        Activity.Current?.AddEvent(new ActivityEvent("Retrieved Users from DB"));

        if (user == null)
        {
            await context.RespondAsync(new FaultedResponse(HttpStatusCode.NotFound, new { Message = "User not found", }));
            return;
        }

        var result = mapper.Map<GetUserByIdQueryResult>(user);
        await context.RespondAsync(result);
    }
}