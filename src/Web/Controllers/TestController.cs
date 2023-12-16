namespace TechStack.Web.Controllers;

using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStack.Application.Common.Models;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TestController : ControllerBase
{
    private readonly IBus bus;
    private readonly IRequestClient<TestQuery> testQueryClient;

    public TestController(IBus bus, IRequestClient<TestQuery> testQueryClient)
    {
        this.bus = bus;
        this.testQueryClient = testQueryClient;
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
    public async Task<IActionResult> CreateTestLock(int id)
    {
        var command = new TestCommand(id);
        var client = bus.CreateRequestClient<TestCommand>();
        var result = await client.GetResponse<TestCommandResponse>(command);

        return Ok(result.Message);
    }

    [AllowAnonymous]
    [HttpPut("{id:int}", Name = "UpdateTestLock")]
    public async Task<IActionResult> UpdateTestLock(int id, [FromBody] UpsertLockCommand model)
    {
        // this doesn't really do anything - just wanted to see the logging
        return NoContent();
    }
}
