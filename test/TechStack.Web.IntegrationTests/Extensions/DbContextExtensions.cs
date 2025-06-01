namespace TechStack.Web.IntegrationTests.Extensions;

using System.Reflection;
using Microsoft.EntityFrameworkCore;

public static class DbContextExtensions
{
    public static bool IsDisposed(this DbContext context)
    {
        var typeDbContext = typeof(DbContext);
        var isDisposedTypeField = typeDbContext.GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance);

        if (isDisposedTypeField != null)
        {
            var rawValue = isDisposedTypeField?.GetValue(context);
            if (rawValue is bool value)
            {
                return value;
            }
        }

        return true;
    }
    
    public static Task EnableIdentityInsert<T>(this DbContext context) => SetIdentityInsert<T>(context, enable: true);
    
    public static Task DisableIdentityInsert<T>(this DbContext context) => SetIdentityInsert<T>(context, enable: false);

    public static void SaveChangesWithIdentityInsert<T>(this DbContext context)
    {
        using var transaction = context.Database.BeginTransaction();
        context.EnableIdentityInsert<T>();
        context.SaveChanges();
        context.DisableIdentityInsert<T>();
        transaction.Commit();
    }

    public static async Task SaveChangesWithIdentityInsertAsync<T>(this DbContext context)
    {
        using var transaction = context.Database.BeginTransaction();
        await context.EnableIdentityInsert<T>();
        await context.SaveChangesAsync();
        await context.DisableIdentityInsert<T>();
        transaction.Commit();
    }
    
    private static Task<int> SetIdentityInsert<T>(DbContext context, bool enable)
    {
        var entityType = context.Model.FindEntityType(typeof(T));
        var value = enable ? "ON" : "OFF";
#pragma warning disable EF1002 // Risk of vulnerability to SQL injection. This is safe to use in Integration Tests, no other way to set Identity Insert in EF Core.
        return context.Database.ExecuteSqlRawAsync(
            $"SET IDENTITY_INSERT {entityType?.GetSchema()}.{entityType?.GetTableName()} {value}");
#pragma warning restore EF1002 // Risk of vulnerability to SQL injection.
    }
}