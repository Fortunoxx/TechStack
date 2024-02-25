namespace TechStack.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using TechStack.Application.Common.Interfaces;

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

    [HttpGet("{id:int}", Name = "GetById")]
    public IActionResult GetById(int id)
    {
        var result = lockService.GetById(id);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost("{id:int}", Name = "CreateLock")]
    public IActionResult CreateLock(int id)
    {
        var data = Guid.NewGuid();
        if (lockService.CreateLock(id, data))
        {
            return CreatedAtRoute("GetById", id, new { Data = data });
        }

        return BadRequest();
    }

    [HttpDelete("{id:int}", Name = "DeleteLock")]
    public IActionResult DeleteLock(int id)
    {
        if (lockService.DeleteLock(id))
            return Ok();
        return NotFound();
    }
}
