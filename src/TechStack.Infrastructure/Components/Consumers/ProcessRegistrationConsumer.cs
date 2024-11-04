namespace TechStack.Infrastructure.Components.Consumers;

using MassTransit;
using MassTransit.Courier.Contracts;
using Microsoft.Extensions.Logging;
using TechStack.Infrastructure.Components.Activities;
using TechStack.Infrastructure.Components.Messaging;
using TechStack.Infrastructure.Contracts;
using TechStack.Infrastructure.Services;

public class ProcessRegistrationConsumer(ILogger<ProcessRegistrationConsumer> logger, IEndpointAddressProvider provider) :
    IConsumer<ProcessRegistration>
{
    readonly ILogger<ProcessRegistrationConsumer> _logger = logger;
    readonly ISecurePaymentInfoService _paymentInfoService = new SecurePaymentInfoService();
    readonly IEndpointAddressProvider _provider = provider;

    public async Task Consume(ConsumeContext<ProcessRegistration> context)
    {
        _logger.LogInformation("Processing registration: {0} ({1})", context.Message.SubmissionId, context.Message.ParticipantEmailAddress);

        var routingSlip = CreateRoutingSlip(context);

        await context.Execute(routingSlip).ConfigureAwait(false);
    }

    RoutingSlip CreateRoutingSlip(ConsumeContext<ProcessRegistration> context)
    {
        var builder = new RoutingSlipBuilder(NewId.NextGuid());

        builder.SetVariables(new
        {
            context.Message.ParticipantEmailAddress,
            context.Message.ParticipantLicenseNumber,
            context.Message.ParticipantCategory,
        });

        if (!string.IsNullOrWhiteSpace(context.Message.ParticipantLicenseNumber))
        {
            builder.AddActivity("LicenseVerification", _provider.GetExecuteEndpoint<LicenseVerificationActivity, LicenseVerificationArguments>(),
                new
                {
                    EventType = "Road",
                });

            builder.AddSubscription(context.SourceAddress, RoutingSlipEvents.ActivityFaulted, RoutingSlipEventContents.None, "LicenseVerification",
                x => x.Send<RegistrationLicenseVerificationFailed>(new { context.Message.SubmissionId }));
        }

        builder.AddActivity("EventRegistration", _provider.GetExecuteEndpoint<EventRegistrationActivity, EventRegistrationArguments>(),
            new
            {
                context.Message.EventId,
                context.Message.RaceId
            });

        var paymentInfo = _paymentInfoService.GetPaymentInfo(context.Message.ParticipantEmailAddress, context.Message.CardNumber);

        builder.AddActivity("ProcessPayment", _provider.GetExecuteEndpoint<ProcessPaymentActivity, ProcessPaymentArguments>(), paymentInfo);

        builder.AddSubscription(context.SourceAddress, RoutingSlipEvents.ActivityFaulted, RoutingSlipEventContents.None, "ProcessPayment",
            x => x.Send<RegistrationPaymentFailed>(new { context.Message.SubmissionId }));

        builder.AddSubscription(context.SourceAddress, RoutingSlipEvents.Completed,
            x => x.Send<RegistrationCompleted>(new { context.Message.SubmissionId }));

        return builder.Build();
    }
}