namespace TechStack.Web.Extensions;

using TechStack.Infrastructure.Data;

/// <summary>
/// This should actually go to infrastructure/data but WebApplication is not known there??
/// </summary>
public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync();
    }
}
