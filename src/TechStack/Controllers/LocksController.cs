using Microsoft.AspNetCore.Mvc;
using TechStack.Services;

namespace TechStack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocksController : ControllerBase
{
    private readonly ILockService lockService;

    public LocksController(ILockService lockService)
        => this.lockService = lockService;

    [HttpGet(Name = "GetAll")]
    public IActionResult GetAll()
    {
        return Ok(lockService.GetAllLocks());
    }

    [HttpPost("{id:int}", Name = "CreateLock")]
    public IActionResult CreateLock(int id)
    {
        if(lockService.CreateLock(id))
            return Created();
        return BadRequest();
    }

    [HttpDelete("{id:int}", Name = "DeleteLock")]
    public IActionResult DeleteLock(int id)
    {
        if (lockService.DeleteLock(id))
            return Ok();
        return BadRequest();
    }
}
