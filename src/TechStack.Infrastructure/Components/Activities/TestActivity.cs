namespace TechStack.Infrastructure.Components.Activities;

using MassTransit;
using Microsoft.Extensions.Logging;

public class TestActivity(ILogger<TestActivity> logger) : IExecuteActivity<TestActivityArguments>
{
    private readonly ILogger<TestActivity> logger = logger;

    public Task<ExecutionResult> Execute(ExecuteContext<TestActivityArguments> context)
    {
        var inputValue = context.Arguments.InputValue;

        logger.LogInformation("Executing TestActivity with InputValue: {InputValue}", inputValue);

        // Simulate some processing logic
        var outputValue = inputValue.ToUpperInvariant();

        return Task.FromResult(context.CompletedWithVariables(new { OutputValue = outputValue, }));
    }
}

public record TestActivityArguments(string InputValue);

