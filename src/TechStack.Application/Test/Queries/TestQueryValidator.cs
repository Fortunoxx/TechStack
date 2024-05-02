namespace TechStack.Application.Test.Queries;

using FluentValidation;

public class TestQueryValidator : AbstractValidator<TestQuery>
{
    public TestQueryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}