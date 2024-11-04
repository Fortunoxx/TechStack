namespace TechStack.Infrastructure.Components.Consumers;

using MassTransit;
using Microsoft.Extensions.Logging;
using TechStack.Infrastructure.Contracts;

public class SubmitRegistrationConsumer(ILogger<SubmitRegistrationConsumer> logger) :
    IConsumer<SubmitRegistration>
{
    readonly ILogger<SubmitRegistrationConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<SubmitRegistration> context)
    {
        _logger.LogInformation("Registration received: {SubmissionId} ({Email})", context.Message.SubmissionId, context.Message.ParticipantEmailAddress);

        ValidateRegistration(context.Message);

        await context.Publish<RegistrationReceived>(context.Message);

        _logger.LogInformation("Registration accepted: {SubmissionId} ({Email})", context.Message.SubmissionId, context.Message.ParticipantEmailAddress);
    }

    void ValidateRegistration(SubmitRegistration message)
    {
        if (string.IsNullOrWhiteSpace(message.EventId))
            throw new ArgumentNullException(nameof(message.EventId));
        if (string.IsNullOrWhiteSpace(message.RaceId))
            throw new ArgumentNullException(nameof(message.RaceId));

        if (string.IsNullOrWhiteSpace(message.ParticipantEmailAddress))
            throw new ArgumentNullException(nameof(message.ParticipantEmailAddress));
        if (string.IsNullOrWhiteSpace(message.ParticipantCategory))
            throw new ArgumentNullException(nameof(message.ParticipantCategory));
    }
}
