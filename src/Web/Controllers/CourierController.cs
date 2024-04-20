namespace TechStack.Web.Controllers;

using MassTransit;
using Microsoft.AspNetCore.Mvc;
using TechStack.Application.Common.Models;

[ApiController]
[Route("api/[controller]")]
public class CourierController : ControllerBase
{
    private readonly IRequestClient<DistributedTransactionCommand> requestClient;

    public CourierController(IRequestClient<DistributedTransactionCommand> requestClient)
        => this.requestClient = requestClient;

    [HttpPost(Name = "StartDistributedTransaction")]
    public async Task<ActionResult<DistributedTransactionOutput>> StartDistributedTransaction([FromBody] DistributedTransactionInput input)
    {
        var command = new DistributedTransactionCommand(input.Key);
        var response = await requestClient.GetResponse<DistributedTransactionResponse>(command);
        var result = new DistributedTransactionOutput(response.Message.Id);
        
        return Ok(result);
    }
}