namespace TechStack.Infrastructure.Contracts;

internal record RetryConfig
{
    public static string SectionName = "RetryConfig";
    public int RetryCount { get; init; }
    public int RetryDelay { get; init; }
}