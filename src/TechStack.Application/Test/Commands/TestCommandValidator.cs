using FluentValidation;

namespace TechStack.Application.Test.Commands;

public class TestCommandValidator : AbstractValidator<TestCommand>
{
    public TestCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}