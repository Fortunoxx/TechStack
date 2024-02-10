namespace TechStack.Web.Controllers;

using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStack.Application.Test.Commands;
using TechStack.Application.Test.Queries;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TestController : ControllerBase
{
    private readonly IRequestClient<TestQuery> testQueryClient;

    private readonly IRequestClient<TestCommand> testCommandClient;

    public TestController(IRequestClient<TestQuery> testQueryClient, IRequestClient<TestCommand> testCommandClient)
    {
        this.testQueryClient = testQueryClient;
        this.testCommandClient = testCommandClient;
    }

    [HttpGet("{id:int}", Name = "GetSomeData")]
    public async Task<IActionResult> GetSomeData(int id)
    {
        var query = new TestQuery(id);
        var result = await testQueryClient.GetResponse<TestQueryResult>(query);

        return Ok(result.Message);
    }

    [AllowAnonymous]
    [HttpGet("anonymous/{id:int}", Name = "GetSomeAnonymousData")]
    public async Task<IActionResult> GetSomeAnonymousData(int id)
    {
        var query = new TestQuery(id);
        var result = await testQueryClient.GetResponse<TestQueryResult>(query);

        return Ok(result.Message);
    }

    [AllowAnonymous]
    [HttpPost("{id:int}", Name = "CreateTestLock")]
    public async Task<IActionResult> CreateTestLock(int id, [FromBody] UpsertLockCommand model)
    {
        var command = new TestCommand(id, model);
        var result = await testCommandClient.GetResponse<TestCommandResponse>(command);

        return Ok(result.Message);
    }

    [AllowAnonymous]
    [HttpPut("{id:int}", Name = "UpdateTestLock")]
    public async Task<IActionResult> UpdateTestLock(int id, [FromBody] UpsertLockCommand model)
    {
        var command = new TestCommand(id, model);
        _ = await testCommandClient.GetResponse<TestCommandResponse>(command);

        return NoContent();
    }
}
