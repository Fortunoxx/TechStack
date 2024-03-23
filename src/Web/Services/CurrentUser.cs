namespace TechStack.Web.Services;

using TechStack.Application.Common.Interfaces;

public class CurrentUser : IUser
{
    const string defaultId = "svc_usr";

    // private readonly IHttpContextAccessor _httpContextAccessor;

    // public CurrentUser(IHttpContextAccessor httpContextAccessor)
    // {
    //     _httpContextAccessor = httpContextAccessor;
    // }

    // public string? Id => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    
    public string? Id => defaultId;
}
