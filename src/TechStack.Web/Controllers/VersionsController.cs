namespace TechStack.Web.Controllers;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

[ApiVersion(1, Deprecated = true)]
[ApiVersion(2)]
[ApiController]
// [Route("api/[controller]")]
// [Route("api/v{version:apiVersion}/[controller]")]
public class VersionsController : ControllerBase
{
    [MapToApiVersion(1)]
    [HttpGet(Name = "Version1")]
    public ActionResult<KeyValuePair<string, string>> GetValueVersion1()
    {
        return new KeyValuePair<string, string>("Version", "1.0");
    }

    [MapToApiVersion(2)]
    [HttpGet(Name = "Version2")]
    public ActionResult<KeyValuePair<Guid, string>> GetValueVersion2()
    {
        return new KeyValuePair<Guid, string>(Guid.NewGuid(), "Version 2.0");
    }
}