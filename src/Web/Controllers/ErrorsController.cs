namespace TechStack.Web.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ErrorsController : ControllerBase
{
    [HttpGet(Name = "Error500")]
    public IActionResult Error500()
    {
        throw new Exception("by design");
    }
}