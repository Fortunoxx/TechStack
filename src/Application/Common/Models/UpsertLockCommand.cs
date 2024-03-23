namespace TechStack.Application.Common.Models;

public record UpsertLockCommand(string Message, DateTime Modified, IDictionary<string, string> KeyValuePairs);
