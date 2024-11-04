namespace TechStack.Infrastructure.Components.Logging;

using Microsoft.Extensions.Logging;
using TechStack.Infrastructure.Components.Activities;

public static partial class Logging
{
    [LoggerMessage(LogLevel.Information, "Assigning waiver")]
    public static partial void LogEmail(this ILogger logger, [LogProperties] AssignWaiverArguments arguments);
}