namespace TechStack.Infrastructure.Contracts;

public interface ISubmissionNotFound
{
    Guid SubmissionId { get; }
    string Status { get; }
}