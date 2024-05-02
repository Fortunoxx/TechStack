namespace TechStack.Web.Controllers;

using MassTransit;
using Microsoft.AspNetCore.Mvc;
using TechStack.Application.Common.Models;
using TechStack.Application.Users.Commands;
using TechStack.Application.Users.Queries;

[ApiController]
[Route("api/[controller]")]
public class UsersController(
    IRequestClient<AddUserCommand> addUserCommandClient,
    IRequestClient<DeleteUserCommand> deleteUserCommandClient,
    IRequestClient<GetUserByIdQuery> getUserByIdQueryClient,
    IRequestClient<GetAllUsersQuery> getAllUsersQueryClient,
    IRequestClient<AlterUserCommand> alterUserCommandClient) : ControllerBase
{
    private readonly IRequestClient<AddUserCommand> addUserCommandClient = addUserCommandClient;
    private readonly IRequestClient<DeleteUserCommand> deleteUserCommandClient = deleteUserCommandClient;
    private readonly IRequestClient<GetUserByIdQuery> getUserByIdQueryClient = getUserByIdQueryClient;
    private readonly IRequestClient<GetAllUsersQuery> getAllUsersQueryClient = getAllUsersQueryClient;
    private readonly IRequestClient<AlterUserCommand> alterUserCommandClient = alterUserCommandClient;

    [HttpGet("{id}", Name = "GetUserById")]
    public async Task<ActionResult<GetUserByIdQueryResult>> GetUserById(int id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await getUserByIdQueryClient.GetResponse<GetUserByIdQueryResult, FaultedResponse>(query);

        if (result.Is<GetUserByIdQueryResult>(out var getUserByIdQueryResult))
        {
            return Ok(getUserByIdQueryResult.Message);
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
            return Ok();
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

    [HttpPut("{id}", Name = "AlterUser")]
    public async Task<IActionResult> AlterUser(int id, [FromBody] AlterUserCommandPart user)
    {
        var result = await alterUserCommandClient.GetResponse<AcceptedResponse, FaultedResponse>(new AlterUserCommand(id, user));

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