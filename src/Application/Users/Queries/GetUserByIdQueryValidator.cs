namespace TechStack.Application.Users.Queries;

using FluentValidation;

public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
