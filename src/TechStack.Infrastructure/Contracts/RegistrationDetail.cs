namespace TechStack.Infrastructure.Contracts;

using MassTransit;

[ExcludeFromTopology]
public record RegistrationDetail
{
    public Guid SubmissionId { get; init; }

    public required string ParticipantEmailAddress { get; init; }

    public required string ParticipantLicenseNumber { get; init; }

    public required string ParticipantCategory { get; init; }

    public required string CardNumber { get; init; }

    public required string EventId { get; init; }

    public required string RaceId { get; init; }
}
