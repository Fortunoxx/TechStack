namespace TechStack.Web;

using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using TechStack.Application.Common.Interfaces;
using TechStack.Infrastructure.Data;
using TechStack.Web.Authentication;
using TechStack.Web.Infrastructure;
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

        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        return services;
    }
}
