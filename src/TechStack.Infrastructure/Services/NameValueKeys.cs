namespace TechStack.Infrastructure.Services;

public static class NameValueKeys
{
    // Define the keys that are expected to be returned by the fetchers, HashSet is used to ensure uniqueness
    public static readonly IReadOnlyCollection<string> Keys = new HashSet<string>
    {
        "Default", "First", "Middle", "Last"
    };
}   