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
    public async Task<IActionResult> GetAll()
    {
        return Ok(await lockService.GetAllLocks());
    }

    [HttpGet("{id:int}", Name = "GetLockById")]
    public async Task<IActionResult> GetLockById(int id)
    {
        var result = await lockService.GetById(id);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost("{id:int}", Name = "CreateLock")]
    public async Task<IActionResult> CreateLock(int id)
    {
        var data = Guid.NewGuid();
        if (await lockService.CreateLock(id, data))
        {
            return CreatedAtRoute("GetLockById", new { id }, new { Data = data });
        }

        return BadRequest();
    }

    [HttpDelete("{id:int}", Name = "DeleteLock")]
    public async Task<IActionResult> DeleteLock(int id)
    {
        if (await lockService.DeleteLock(id))
            return Ok();
        return NotFound();
    }
}
