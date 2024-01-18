using TechStack.Application.Common.Interfaces;

namespace TechStack.Application.Test.Commands;

public record TestCommand(int Id, object Data, string CorrelationId) : ICorrelationId;
