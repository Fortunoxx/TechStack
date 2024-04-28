using System.Net;
using MassTransit;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Common.Models;

namespace TechStack.Application.Users.Commands;

public class DeleteUserCommandConsumer : IConsumer<DeleteUserCommand>
{
    private readonly IApplicationDbContext applicationDbContext;

    public DeleteUserCommandConsumer(IApplicationDbContext applicationDbContext) => this.applicationDbContext = applicationDbContext;

    public async Task Consume(ConsumeContext<DeleteUserCommand> context)
    {
        var user = await applicationDbContext.Users.FindAsync(context.Message.Id);

        if (user != null)
        {
            applicationDbContext.Users.Remove(user);
            await applicationDbContext.SaveChangesAsync(context.CancellationToken);
            await context.RespondAsync(new AcceptedResponse());
            return;
        }

        await context.RespondAsync(new FaultedResponse(HttpStatusCode.NotFound, new { Message = "User not found", }));
    }
}
