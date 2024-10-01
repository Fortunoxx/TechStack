namespace TechStack.Infrastructure.Components.Activities;

public record LicenseVerificationArguments
{
    /// <summary>
    /// Racer's license number
    /// </summary>
    public required string ParticipantLicenseNumber { get; init; }

    public required string EventType { get; init; }
    
    public required string ParticipantCategory { get; init; }
}