namespace TechStack.Web.Controllers;

using MassTransit;
using Microsoft.AspNetCore.Mvc;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Users.Queries;

[ApiController]
[Route("api/[controller]")]
public class FetcherController(ICorrelationIdGenerator correlationIdGenerator, IRequestClient<FetchDataQuery> requestClient) : ControllerBase
{
    private readonly IRequestClient<FetchDataQuery> requestClient = requestClient;

    [HttpGet(Name = "FetchDataFromDifferentSources")]
    public async Task<ActionResult<IDictionary<string, string>>> GetDataAsync()
    {
        var query = new FetchDataQuery();
        var response = await requestClient.GetResponse<FetchDataQueryResult>(query);

        return Ok(response.Message.Data);
    }
}
