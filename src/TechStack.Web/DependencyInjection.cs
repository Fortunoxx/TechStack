namespace TechStack.Web;

using TechStack.Application.Common.Interfaces;
using TechStack.Infrastructure;
using TechStack.Web.Authentication;
using TechStack.Web.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        services.AddScoped<ApiKeyAuthFilter>();
        services.AddScoped<IApiKeyValidation, ApiKeyValidation>();
        services.AddScoped<IUser, CurrentUser>();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        return services;
    }
}