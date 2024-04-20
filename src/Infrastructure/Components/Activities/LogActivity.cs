using MassTransit;
using Microsoft.Extensions.Logging;

namespace TechStack.Infrastructure.Components.Activities;

public class LogActivity : IActivity<LogActivityArguments, LogActivityLog>
{
    private readonly ILogger<LogActivity> logger;

    public LogActivity(ILogger<LogActivity> logger)
        => this.logger = logger;

    public async Task<CompensationResult> Compensate(CompensateContext<LogActivityLog> context)
    {
        logger.LogError("Cannot compensate {@Entries}", context.Log.Entries);
        return context.Compensated();
    }

    public async Task<ExecutionResult> Execute(ExecuteContext<LogActivityArguments> context)
    {
        var (message, level) = context.Arguments;
        logger.Log(level, message);
        var entries = new[] { message, };
        return context.Completed(new LogActivityLog(entries));
    }
}

public record LogActivityLog(IEnumerable<string> Entries);

public record LogActivityArguments(string Message, LogLevel LogLevel = LogLevel.Debug);