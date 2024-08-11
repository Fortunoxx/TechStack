namespace TechStack.Infrastructure.Contracts;

using System.Runtime.CompilerServices;
using MassTransit;
using MassTransit.Courier.Contracts;

[ExcludeFromTopology]
public record RegistrationDetail
{
    public Guid SubmissionId { get; init; }

    public string ParticipantEmailAddress { get; init; }
    public string ParticipantLicenseNumber { get; init; }
    public string ParticipantCategory { get; init; }

    public string CardNumber { get; init; }

    public string EventId { get; init; }
    public string RaceId { get; init; }
}

public record SubmitRegistration : RegistrationDetail;
public record RegistrationReceived : RegistrationDetail;
public record ProcessRegistration : RegistrationDetail;
public static class CorrelationInitializer
{
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    public static void Initialize()
    {
        MessageCorrelation.UseCorrelationId<GetRegistrationStatus>(x => x.SubmissionId);
        MessageCorrelation.UseCorrelationId<ProcessRegistration>(x => x.SubmissionId);
        MessageCorrelation.UseCorrelationId<RegistrationStatus>(x => x.SubmissionId);
        MessageCorrelation.UseCorrelationId<RegistrationCompleted>(x => x.SubmissionId);
        MessageCorrelation.UseCorrelationId<RegistrationLicenseVerificationFailed>(x => x.SubmissionId);
        MessageCorrelation.UseCorrelationId<RegistrationPaymentFailed>(x => x.SubmissionId);
        MessageCorrelation.UseCorrelationId<RegistrationReceived>(x => x.SubmissionId);
        MessageCorrelation.UseCorrelationId<SubmitRegistration>(x => x.SubmissionId);
    }
}

public record GetRegistrationStatus
{
    public Guid SubmissionId { get; init; }
}

public record RegistrationCompleted : RoutingSlipCompleted
{
    public Guid SubmissionId { get; init; }
    public Guid TrackingNumber { get; init; }

    public DateTime Timestamp { get; set; }
    public TimeSpan Duration { get; set; }
    public IDictionary<string, object> Variables { get; set; }
}

public record RegistrationLicenseVerificationFailed
{
    public Guid SubmissionId { get; init; }

    public ExceptionInfo ExceptionInfo { get; init; }
}

public record RegistrationPaymentFailed
{
    public Guid SubmissionId { get; init; }

    public ExceptionInfo ExceptionInfo { get; init; }
}

public record RegistrationStatus :
    RegistrationDetail
{
    public string Status { get; init; }
    public DateTime? ParticipantLicenseExpirationDate { get; init; }
    public Guid? RegistrationId { get; init; }
}

public record RetryDelayExpired(Guid RegistrationId);


