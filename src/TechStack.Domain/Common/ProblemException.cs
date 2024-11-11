namespace TechStack.Domain.Common;

public class ProblemException(string error, string? message) : Exception(message)
{
    private readonly string error = error;

    public string Error => error;
}
