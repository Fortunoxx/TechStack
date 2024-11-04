namespace TechStack.Infrastructure.Contracts;

using MassTransit;

public record RegistrationLicenseVerificationFailed
{
    public Guid SubmissionId { get; init; }

    public required ExceptionInfo ExceptionInfo { get; init; }
}
