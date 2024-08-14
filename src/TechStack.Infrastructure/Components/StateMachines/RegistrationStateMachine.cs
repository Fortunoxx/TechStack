namespace TechStack.Infrastructure.Components.StateMachines;

using MassTransit;
using TechStack.Infrastructure.Contracts;

public class RegistrationStateMachine : MassTransitStateMachine<RegistrationState>
{
    public RegistrationStateMachine()
    {
        InstanceState(x => x.CurrentState);

        SetCompletedWhenFinalized();

        Event(() => RegistrationStatusRequested, eventConfig =>
        {
            eventConfig.ReadOnly = true;

            // eventConfig.OnMissingInstance(m => m.Fault()); // ! this moves the message to the error-queue
            eventConfig.OnMissingInstance(config => config.
                ExecuteAsync(context => context.
                    RespondAsync<ISubmissionNotFound>(
                        new { context.Message.SubmissionId, Status = "Not found", }
                    )
                )
            );
        });

        Schedule(() => RetryDelayExpired, saga => saga.ScheduleRetryToken, x =>
        {
            x.Received = r =>
            {
                r.CorrelateById(context => context.Message.SubmissionId);
                r.ConfigureConsumeTopology = false;
            };
        });

        Initially(
            When(EventRegistrationReceived)
                .Initialize()
                .InitiateProcessing()
                .TransitionTo(Received));

        During(Received,
            When(EventRegistrationCompleted)
                .Registered()
                .TransitionTo(Registered),
            When(LicenseVerificationFailed)
                .InvalidLicense()
                .TransitionTo(Suspended),
            When(PaymentFailed)
                .PaymentFailed()
                .TransitionTo(Suspended));

        During(Suspended,
            When(EventRegistrationReceived)
                .Initialize()
                .InitiateProcessing()
                .TransitionTo(Received));

        During(WaitingToRetry,
            When(RetryDelayExpired?.Received)
                .RetryProcessing()
                .TransitionTo(Received));

        DuringAny(
            When(RegistrationStatusRequested)
                .Respond(x => new RegistrationStatus
                {
                    SubmissionId = x.Saga.CorrelationId,
                    ParticipantEmailAddress = x.Saga.ParticipantEmailAddress,
                    ParticipantLicenseNumber = x.Saga.ParticipantLicenseNumber,
                    ParticipantLicenseExpirationDate = x.Saga.ParticipantLicenseExpirationDate,
                    RegistrationId = x.Saga.RegistrationId,
                    EventId = x.Saga.EventId,
                    RaceId = x.Saga.RaceId,
                    Status = x.Saga.CurrentState,
                    ParticipantCategory = x.Saga.ParticipantCategory,
                    CardNumber = x.Saga.CardNumber,
                })
        );

        // could easily be configured via options
        const int retryCount = 2;
        var retryDelay = TimeSpan.FromSeconds(2);

        WhenEnter(Suspended, x => x
            .Then(y => LogContext.Info?.Log("Retrying {RetryAttempt} {SubmissionId}", y.Saga.RetryAttempt, y.Saga.CorrelationId))
            .IfElse(context => context.Saga.RetryAttempt < retryCount,
                retry => retry
                    .Schedule(RetryDelayExpired, context => new RetryDelayExpired(context.Saga.CorrelationId), _ => retryDelay)
                    .TransitionTo(WaitingToRetry),
                otherwise => otherwise
                    .Then(y => LogContext.Info?.Log("Creating Ticket with this data {@Saga}", y.Saga))
                    .PublishAsync(context => context.Init<CreateTicketRequest>(new
                    {
                        SubmissionId = context.Saga.CorrelationId,
                        context.Saga.ParticipantEmailAddress,
                        context.Saga.ParticipantLicenseNumber,
                        context.Saga.ParticipantCategory,
                        context.Saga.CardNumber,
                        context.Saga.EventId,
                        context.Saga.RaceId,
                    }))
                    .Finalize()
            ));
    } // ReSharper disable UnassignedGetOnlyAutoProperty
      // ReSharper disable MemberCanBePrivate.Global
    public State? Moved { get; }
    public State? Received { get; }
    public State? Registered { get; }
    public State? WaitingToRetry { get; }
    public State? Suspended { get; }

    public Event<RegistrationReceived>? EventRegistrationReceived { get; }
    public Event<GetRegistrationStatus>? RegistrationStatusRequested { get; }
    public Event<RegistrationCompleted>? EventRegistrationCompleted { get; }
    public Event<RegistrationLicenseVerificationFailed>? LicenseVerificationFailed { get; }
    public Event<RegistrationPaymentFailed>? PaymentFailed { get; }
    public Schedule<RegistrationState, RetryDelayExpired>? RetryDelayExpired { get; }
}

static class RegistrationStateMachineBehaviorExtensions
{
    public static EventActivityBinder<RegistrationState, RegistrationReceived> Initialize(
        this EventActivityBinder<RegistrationState, RegistrationReceived> binder)
    {
        return binder.Then(context =>
        {
            context.Saga.ParticipantEmailAddress = context.Message.ParticipantEmailAddress;
            context.Saga.ParticipantLicenseNumber = context.Message.ParticipantLicenseNumber;
            context.Saga.ParticipantCategory = context.Message.ParticipantCategory;
            context.Saga.CardNumber = context.Message.CardNumber;

            context.Saga.EventId = context.Message.EventId;
            context.Saga.RaceId = context.Message.RaceId;

            LogContext.Info?.Log("Processing: {SubmissionId} ({ParticipantEmailAddress})", context.Message.SubmissionId, context.Message.ParticipantEmailAddress);
        });
    }

    public static EventActivityBinder<RegistrationState, RegistrationReceived> InitiateProcessing(
        this EventActivityBinder<RegistrationState, RegistrationReceived> binder)
    {
        return binder.PublishAsync(context => context.Init<ProcessRegistration>(context.Message));
    }

    public static EventActivityBinder<RegistrationState, RetryDelayExpired> RetryProcessing(
        this EventActivityBinder<RegistrationState, RetryDelayExpired> binder)
    {
        return binder
            .Then(context => context.Saga.RetryAttempt++)
            .PublishAsync(context => context.Init<ProcessRegistration>(new
            {
                SubmissionId = context.Saga.CorrelationId,
                context.Saga.ParticipantEmailAddress,
                context.Saga.ParticipantLicenseNumber,
                context.Saga.ParticipantCategory,
                context.Saga.CardNumber,
                context.Saga.EventId,
                context.Saga.RaceId,
                __Header_Registration_RetryAttempt = context.Saga.RetryAttempt
            }));
    }

    public static EventActivityBinder<RegistrationState, RegistrationCompleted> Registered(
        this EventActivityBinder<RegistrationState, RegistrationCompleted> binder)
    {
        return binder.Then(context =>
        {
            LogContext.Info?.Log("Registered: {SubmissionId} ({ParticipantEmailAddress})", context.Message.SubmissionId, context.Saga.ParticipantEmailAddress);

            context.Saga.ParticipantLicenseExpirationDate = context.GetVariable<DateTime>("ParticipantLicenseExpirationDate");
            context.Saga.RegistrationId = context.GetVariable<Guid>("RegistrationId");
        });
    }

    public static EventActivityBinder<RegistrationState, RegistrationLicenseVerificationFailed> InvalidLicense(
        this EventActivityBinder<RegistrationState, RegistrationLicenseVerificationFailed> binder)
    {
        return binder.Then(context =>
        {
            LogContext.Info?.Log("Invalid License: {SubmissionId} ({ParticipantLicenseNumber}) - {Message}", context.Message.SubmissionId, context.Saga.ParticipantLicenseNumber,
                context.Message.ExceptionInfo.Message);

            context.Saga.Reason = "Invalid License";
        });
    }

    public static EventActivityBinder<RegistrationState, RegistrationPaymentFailed> PaymentFailed(
        this EventActivityBinder<RegistrationState, RegistrationPaymentFailed> binder)
    {
        return binder.Then(context =>
        {
            LogContext.Info?.Log("Payment Failed: {SubmissionId} ({ParticipantEmailAddress}) - {Message}", context.Message.SubmissionId, context.Saga.ParticipantEmailAddress,
                context.Message?.ExceptionInfo?.Message);

            context.Saga.Reason = "Payment Failed";
        });
    }
}
