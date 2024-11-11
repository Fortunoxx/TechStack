namespace TechStack.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using TechStack.Domain.Common;

[ApiController]
[Route("api/[controller]")]
public class ErrorsController : ControllerBase
{
    [HttpGet(Name = "Error500")]
    public IActionResult Error500()
    {
        throw new ProblemException("You found an error", "this happened by design")
        {
            Data =
            {
                { "information", "you should not use the reserved keywords 'requestId' and 'traceId'" },
            }
        };
    }
}