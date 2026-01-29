using System.Diagnostics.CodeAnalysis;

namespace TechStack.Application.Common.Models;

[ExcludeFromCodeCoverage]
public record DistributedTransactionInput(string Key);

[ExcludeFromCodeCoverage]
public record DistributedTransactionOutput(Guid? Id);

[ExcludeFromCodeCoverage]
public record DistributedTransactionCommand(string Key);

[ExcludeFromCodeCoverage]
public record DistributedTransactionResponse(Guid? Id);