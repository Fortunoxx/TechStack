namespace TechStack.Application.Test.Commands;

public record TestCommand
{
    public int Id { get; init; }

    public required object Data { get; init; }
}
