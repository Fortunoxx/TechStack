namespace TechStack.Infrastructure.Contracts;

public record GetRegistrationStatus
{
    public Guid SubmissionId { get; init; }
}
