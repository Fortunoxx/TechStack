namespace TechStack.Web.Controllers;

using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using TechStack.Application.Common.Models;
using TechStack.Application.Users.Queries;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IRequestClient<GetUserByIdQuery> getUserByIdQueryClient;

    public UsersController(IRequestClient<GetUserByIdQuery> getUserByIdQueryClient)
    {
        this.getUserByIdQueryClient = getUserByIdQueryClient;
    }

    [HttpGet("{id}", Name = "GetUserById")]
    public async Task<ActionResult<GetUserByIdQueryResult>> GetUserById(int id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await getUserByIdQueryClient.GetResponse<GetUserByIdQueryResult, FaultedMessage>(query);

        if (result.Is<GetUserByIdQueryResult>(out var getUserByIdQueryResult))
        {
            return Ok(getUserByIdQueryResult.Message);
        }

        if (result.Is<FaultedMessage>(out var faultedMessage))
        {
            return new ObjectResult(faultedMessage.Message.Payload)
            {
                StatusCode = faultedMessage.Message.HttpStatusCode,
            };
        }

        return BadRequest();
    }
}