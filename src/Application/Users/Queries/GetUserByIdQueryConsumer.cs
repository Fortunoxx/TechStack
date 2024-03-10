namespace TechStack.Application.Users.Queries;

using System.Net;
using AutoMapper;
using MassTransit;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Common.Models;

public class GetUserByIdQueryConsumer : IConsumer<GetUserByIdQuery>
{
    private readonly IApplicationDbContext applicationDbContext;
    private readonly IMapper mapper;

    public GetUserByIdQueryConsumer(IApplicationDbContext applicationDbContext, IMapper mapper)
    {
        this.applicationDbContext = applicationDbContext;
        this.mapper = mapper;
    }

    public async Task Consume(ConsumeContext<GetUserByIdQuery> context)
    {
        var user = await applicationDbContext.Users.FindAsync(context.Message.Id);

        if (user == null)
        {
            await context.RespondAsync(new FaultedMessage((int)HttpStatusCode.NotFound, new { Message = "User not found", }));
            return;
        }

        var result = mapper.Map<GetUserByIdQueryResult>(user);
        await context.RespondAsync(result);
    }
}