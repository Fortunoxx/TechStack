public record UpsertLockCommand(string Message, DateTime Modified, IDictionary<string, string> KeyValuePairs);
