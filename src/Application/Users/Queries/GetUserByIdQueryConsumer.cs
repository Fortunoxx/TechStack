namespace TechStack.Application.Users.Queries;

using System.Net;
using AutoMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Common.Models;

public class GetUserByIdQueryConsumer(IApplicationDbContext applicationDbContext, IMapper mapper) : IConsumer<GetUserByIdQuery>
{
    private readonly IApplicationDbContext applicationDbContext = applicationDbContext;
    private readonly IMapper mapper = mapper;

    public async Task Consume(ConsumeContext<GetUserByIdQuery> context)
    {
        var user = await applicationDbContext.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == context.Message.Id);

        if (user == null)
        {
            await context.RespondAsync(new FaultedResponse(HttpStatusCode.NotFound, new { Message = "User not found", }));
            return;
        }

        var result = mapper.Map<GetUserByIdQueryResult>(user);
        await context.RespondAsync(result);
    }
}