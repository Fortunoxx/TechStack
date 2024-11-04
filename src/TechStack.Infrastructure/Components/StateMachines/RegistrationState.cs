namespace TechStack.Infrastructure.Components.StateMachines;

using MassTransit;

public class RegistrationState : SagaStateMachineInstance, ISagaVersion
{
    public required string ParticipantEmailAddress { get; set; }
    public required string ParticipantLicenseNumber { get; set; }
    public required string ParticipantCategory { get; set; }

    public DateTime? ParticipantLicenseExpirationDate { get; set; }
    public Guid? RegistrationId { get; set; }

    public required string CardNumber { get; set; }

    public required string EventId { get; set; }

    public required string RaceId { get; set; }

    public required string CurrentState { get; set; }

    public required string Reason { get; set; }

    public int RetryAttempt { get; set; }
    public Guid? ScheduleRetryToken { get; set; }

    public Guid CorrelationId { get; set; }

    public int Version { get; set; }
}