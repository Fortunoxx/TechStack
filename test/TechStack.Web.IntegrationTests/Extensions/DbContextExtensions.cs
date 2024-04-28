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
}