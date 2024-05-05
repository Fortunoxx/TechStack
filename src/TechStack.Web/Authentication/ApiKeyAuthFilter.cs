namespace TechStack.Web.Authentication;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ApiKeyAuthFilter(IApiKeyValidation apiKeyValidation) : IAuthorizationFilter
{
    private readonly IApiKeyValidation _apiKeyValidation = apiKeyValidation;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        string userApiKey = context.HttpContext.Request.Headers[AuthConstants.ApiKeyHeaderName].ToString();

        if (string.IsNullOrWhiteSpace(userApiKey))
        {
            context.Result = new UnauthorizedObjectResult("API Key missing");
            return;
        }

        if (!_apiKeyValidation.IsValidApiKey(userApiKey))
        {
            context.Result = new UnauthorizedObjectResult("Invalid API Key");
        }
    }
}
