namespace TechStack.Infrastructure.Components.Activities;

using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using TechStack.Infrastructure.Components.Logging;

public class AssignWaiverActivity(ILogger<AssignWaiverActivity> logger) :
    IExecuteActivity<AssignWaiverArguments>
{
    readonly ILogger<AssignWaiverActivity> _logger = logger;

    public async Task<ExecutionResult> Execute(ExecuteContext<AssignWaiverArguments> context)
    {
        var arguments = context.Arguments;

        var emailAddress = arguments.ParticipantEmailAddress;

        _logger.LogEmail(arguments);

        await Task.Delay(10);   

        if (emailAddress == "joey@friends.tv")
            throw new RoutingSlipException($"The document server failed to respond: {emailAddress}");

        return context.Completed();
    }
}