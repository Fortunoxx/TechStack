namespace TechStack.Web.Controllers;

using MassTransit;
using Microsoft.AspNetCore.Mvc;
using TechStack.Application.Common.Models;
using TechStack.Application.Users.Commands;
using TechStack.Application.Users.Queries;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IRequestClient<AddUserCommand> addUserCommandClient;
    private readonly IRequestClient<DeleteUserCommand> deleteUserCommandClient;
    private readonly IRequestClient<GetUserByIdQuery> getUserByIdQueryClient;
    private readonly IRequestClient<GetAllUsersQuery> getAllUsersQueryClient;

    public UsersController(
        IRequestClient<AddUserCommand> addUserCommandClient,
        IRequestClient<DeleteUserCommand> deleteUserCommandClient,
        IRequestClient<GetUserByIdQuery> getUserByIdQueryClient,
        IRequestClient<GetAllUsersQuery> getAllUsersQueryClient)
    {
        this.addUserCommandClient = addUserCommandClient;
        this.deleteUserCommandClient = deleteUserCommandClient;
        this.getUserByIdQueryClient = getUserByIdQueryClient;
        this.getAllUsersQueryClient = getAllUsersQueryClient;
    }

    [HttpGet("{id}", Name = "GetUserById")]
    public async Task<ActionResult<GetUserByIdQueryResult>> GetUserById(int id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await getUserByIdQueryClient.GetResponse<GetUserByIdQueryResult, FaultedResponse>(query);

        if (result.Is<GetUserByIdQueryResult>(out var getUserByIdQueryResult))
        {
            return Ok(getUserByIdQueryResult.Message);
        }

        if (result.Is<FaultedResponse>(out var faultedMessage))
        {
            return new ObjectResult(faultedMessage.Message.Payload)
            {
                StatusCode = (int)faultedMessage.Message.HttpStatusCode,
            };
        }

        return BadRequest();
    }

    [HttpGet(Name = "GetAllUsers")]
    public async Task<ActionResult<GetUserByIdQueryResult>> GetAllUsers()
    {
        var result = await getAllUsersQueryClient.GetResponse<GetAllUsersQueryResult, FaultedResponse>(new GetAllUsersQuery());

        if (result.Is<GetAllUsersQueryResult>(out var getAllUsersQueryResult))
        {
            return Ok(getAllUsersQueryResult.Message);
        }

        if (result.Is<FaultedResponse>(out var faultedMessage))
        {
            return new ObjectResult(faultedMessage.Message.Payload)
            {
                StatusCode = (int)faultedMessage.Message.HttpStatusCode,
            };
        }

        return BadRequest();
    }

    [HttpPost(Name = "AddUser")]
    public async Task<IActionResult> AddUser([FromBody] AddUserCommand insertUserCommand)
    {
        var result = await addUserCommandClient.GetResponse<AddUserCommandResponse, FaultedResponse>(insertUserCommand);

        if (result.Is<AddUserCommandResponse>(out var addUserCommandResponse))
        {
            return CreatedAtAction(nameof(GetUserById), new { id = addUserCommandResponse.Message.Id, }, addUserCommandResponse.Message);
        }

        if (result.Is<FaultedResponse>(out var faultedMessage))
        {
            return new ObjectResult(faultedMessage.Message.Payload)
            {
                StatusCode = (int)faultedMessage.Message.HttpStatusCode,
            };
        }

        return BadRequest();
    }

    [HttpDelete("{id}", Name = "DeleteUser")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await deleteUserCommandClient.GetResponse<AcceptedResponse, FaultedResponse>(new DeleteUserCommand(id));

        if (result.Is<AcceptedResponse>(out _))
        {
            return NoContent();
        }

        if (result.Is<FaultedResponse>(out var faultedMessage))
        {
            return new ObjectResult(faultedMessage.Message.Payload)
            {
                StatusCode = (int)faultedMessage.Message.HttpStatusCode,
            };
        }

        return BadRequest();
    }
}