using System.Diagnostics.CodeAnalysis;

namespace TechStack.Application.Common.Models;

[ExcludeFromCodeCoverage]
public record UpsertLockCommand(string Message, DateTime Modified, IDictionary<string, string> KeyValuePairs);
