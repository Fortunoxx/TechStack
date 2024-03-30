namespace TechStack.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using TechStack.Application.Common.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class LocksController(IConfidentialDataService confidentialDataService,
    ILockService lockService,
    ILogger<LocksController> logger) : ControllerBase
{
    private readonly IConfidentialDataService confidentialDataService = confidentialDataService;
    private readonly ILockService lockService = lockService;
    private readonly ILogger<LocksController> logger = logger;

    [HttpGet(Name = "GetAll")]
    public IActionResult GetAll()
    {
        return Ok(lockService.GetAllLocks());
    }

    [HttpGet("{id:int}", Name = "GetLockById")]
    public IActionResult GetLockById(int id)
    {
        var result = lockService.GetById(id);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost("{id:int}", Name = "CreateLock")]
    public IActionResult CreateLock(int id)
    {
        var data = confidentialDataService.CreateEntry();
        logger.LogConfidentialDataCreated(data);

        if (lockService.CreateLock(id, data))
        {
            return CreatedAtRoute("GetLockById", new { id, }, new { Data = data });
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
