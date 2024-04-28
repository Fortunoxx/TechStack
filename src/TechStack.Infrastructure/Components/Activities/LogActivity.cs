namespace TechStack.Infrastructure.Components.Activities;

using MassTransit;
using Microsoft.Extensions.Logging;

public class LogActivity(ILogger<LogActivity> logger) : IActivity<LogActivityArguments, LogActivityLog>
{
    private readonly ILogger<LogActivity> logger = logger;

    public Task<CompensationResult> Compensate(CompensateContext<LogActivityLog> context)
    {
        logger.LogError("Cannot compensate {@Entries}", context.Log.Entries);
        return Task.FromResult(context.Compensated());
    }

    public Task<ExecutionResult> Execute(ExecuteContext<LogActivityArguments> context)
    {
        var (message, level) = context.Arguments;
        
        var entries = new List<string>();
        if(logger.IsEnabled(level))
        {
            #pragma warning disable CA2254 // This behavior is intended here
            logger.Log(level, message);
            #pragma warning restore CA2254 // This behavior is intended here

            entries.Add(message);
        }

        return Task.FromResult(context.Completed(new LogActivityLog(entries)));
    }
}

public record LogActivityLog(IEnumerable<string> Entries);

public record LogActivityArguments(string Message, LogLevel LogLevel = LogLevel.Debug);