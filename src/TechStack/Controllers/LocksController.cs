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

    [HttpPost("{id:int}/set", Name = "AquireLock")]
    public IActionResult AquireLock(int id)
    {
        if(lockService.AquireLock(id))
            return Ok();
        return BadRequest();
    }

    [HttpPost("{id:int}/release", Name = "ReleaseLock")]
    public IActionResult ReleaseLock(int id)
    {
        if (lockService.ReleaseLock(id))
            return Ok();
        return BadRequest();
    }
}
