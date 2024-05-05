namespace TechStack.Web.Authentication;

using System.Net;

public class ApiKeyAuthMiddleware(RequestDelegate next, IApiKeyValidation apiKeyValidation)
{
    private readonly RequestDelegate _next = next;
    private readonly IApiKeyValidation _apiKeyValidation = apiKeyValidation;

    public async Task InvokeAsync(HttpContext context)
    {
        string? userApiKey = context.Request.Headers[AuthConstants.ApiKeyHeaderName];

        if (string.IsNullOrWhiteSpace(userApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsync("API Key missing");
            return;
        }

        if (!_apiKeyValidation.IsValidApiKey(userApiKey!))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Invalid API Key");
            return;
        }

        await _next(context);
    }
}
