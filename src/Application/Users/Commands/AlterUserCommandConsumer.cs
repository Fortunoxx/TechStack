using System.Net;
using AutoMapper;
using MassTransit;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Common.Models;

namespace TechStack.Application.Users.Commands;

public class AlterUserCommandConsumer(IApplicationDbContext applicationDbContext, IMapper mapper) : IConsumer<AlterUserCommand>
{
    private readonly IApplicationDbContext applicationDbContext = applicationDbContext;
    private readonly IMapper mapper = mapper;

    public async Task Consume(ConsumeContext<AlterUserCommand> context)
    {
        var user = await applicationDbContext.Users.FindAsync(context.Message.Id);
        var _ = mapper.Map(context.Message.User, user!);

        var rowsAffected = await applicationDbContext.SaveChangesAsync(context.CancellationToken);

        if (rowsAffected > 0)
        {
            await context.RespondAsync(new AcceptedResponse());
            return;
        }

        await context.RespondAsync(new FaultedResponse(HttpStatusCode.InternalServerError, new { Message = "Could not update data", }));
    }
}
