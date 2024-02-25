namespace TechStack.Application.Test.Commands;

public record TestCommand
{
    public int Id { get; init; }

    public object Data { get; init; }
}
