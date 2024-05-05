namespace TechStack.Web.Authentication;

using Microsoft.AspNetCore.Mvc;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthenticationAttribute : ServiceFilterAttribute
{
    public ApiKeyAuthenticationAttribute() : base(typeof(ApiKeyAuthFilter)) { }
}
