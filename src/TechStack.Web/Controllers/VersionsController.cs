namespace TechStack.Web.Controllers;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TechStack.Web.Infrastructure;

[ApiVersion(1.0, Deprecated = true)]
[ApiVersion(2.0)]
[ApiVersion(3.0)]
[ApiController]
[Route("api/[controller]")]
public class VersionsController(IOptionsMonitor<AppSettings> appSettings) : ControllerBase
{
    private readonly IOptionsMonitor<AppSettings> appSettings = appSettings;

    [MapToApiVersion(1.0)]
    [HttpGet(Name = "GetValueVersion1")]
    public ActionResult<KeyValuePair<string, string>> GetValueVersion1()
    {
        return new KeyValuePair<string, string>("Version", "1.0");
    }

    [MapToApiVersion(2.0)]
    [HttpGet(Name = "GetValueVersion2")]
    public ActionResult<KeyValuePair<Guid, string>> GetValueVersion2()
    {
        return new KeyValuePair<Guid, string>(Guid.NewGuid(), "Version 2.0");
    }

    [MapToApiVersion(3.0)]
    [HttpGet(Name = "GetValueVersion3")]
    public ActionResult<KeyValuePair<Guid, string>> GetValueVersion3()
    {
        var defaultVersion = appSettings.CurrentValue.DefaultVersion;
        return new KeyValuePair<Guid, string>(Guid.NewGuid(), $"{defaultVersion}");
    }
}
