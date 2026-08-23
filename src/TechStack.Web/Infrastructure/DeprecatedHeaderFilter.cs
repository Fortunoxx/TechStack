namespace TechStack.Web.Infrastructure;

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

public class DeprecatedHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= [];

        if (context.ApiDescription.IsDeprecated())
        {
            operation.Deprecated = true;
        }
    }
}