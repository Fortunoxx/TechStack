namespace TechStack.Web.Controllers;

using MassTransit;
using Microsoft.AspNetCore.Mvc;
using TechStack.Application.Common.Interfaces;
using TechStack.Application.Common.Models;
using TechStack.Web.Authentication;

[ApiController]
[ApiKeyAuthentication]
[Route("api/[controller]")]
public class CourierController(ICorrelationIdGenerator correlationIdGenerator, IRequestClient<DistributedTransactionCommand> requestClient) : ControllerBase
{
    private readonly IRequestClient<DistributedTransactionCommand> requestClient = requestClient;

    [HttpPost(Name = "StartDistributedTransaction")]
    public async Task<ActionResult<DistributedTransactionOutput>> StartDistributedTransaction([FromBody] DistributedTransactionInput input)
    {
        var command = new DistributedTransactionCommand(input.Key, correlationIdGenerator.Get());
        var response = await requestClient.GetResponse<DistributedTransactionResponse>(command);
        var result = new DistributedTransactionOutput(response.Message.Id);

        return Ok(result);
    }
}
