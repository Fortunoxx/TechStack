namespace TechStack.Application.Common.Models;

public record DistributedTransactionInput(string Key);

public record DistributedTransactionOutput(Guid? Id);

public record DistributedTransactionCommand(string Key, string CorrelationId);

public record DistributedTransactionResponse(Guid? Id);