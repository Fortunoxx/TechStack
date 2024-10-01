namespace TechStack.Infrastructure.Components.Activities;

using System;


public record EventRegistrationArguments
{
    public required string ParticipantEmailAddress { get; init; }

    public required string ParticipantLicenseNumber { get; init; }
    
    public DateTime? ParticipantLicenseExpirationDate { get; init; }

    public required string ParticipantCategory { get; init; }

    public required string EventId { get; init; }

    public required string RaceId { get; init; }
}