using FluentValidation;

namespace TechStack.Application.Test.Queries;

public class TestQueryValidator : AbstractValidator<TestQuery>
{
    public TestQueryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}