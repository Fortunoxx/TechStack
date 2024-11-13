using System.Net;
using AutoMapper;
using MassTransit;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Common.Models;
using TechStack.Domain.Entities;

namespace TechStack.Application.Users.Commands;

public class AddUserCommandConsumer(IApplicationDbContext applicationDbContext, IMapper mapper) : IConsumer<AddUserCommand>
{
    private readonly IApplicationDbContext applicationDbContext = applicationDbContext;
    private readonly IMapper mapper = mapper;

    public async Task Consume(ConsumeContext<AddUserCommand> context)
    {
        var user = mapper.Map<User>(context.Message);

        applicationDbContext.Users.Add(user);
        var rowsInserted = await applicationDbContext.SaveChangesAsync(context.CancellationToken);

        if (rowsInserted > 0)
        {
            var response = new AddUserCommandResponse(user.Id);
            await context.RespondAsync(response);
            return;
        }

        await context.RespondAsync(new FaultedResponse(HttpStatusCode.InternalServerError, new { Message = "Could not insert data", }));
    }
}
