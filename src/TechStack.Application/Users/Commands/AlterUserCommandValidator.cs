namespace TechStack.Application.Users.Commands;

using FluentValidation;

public class AlterUserCommandValidator : AbstractValidator<AlterUserCommand>
{
    public AlterUserCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.User).NotEmpty();
        RuleFor(x => x.User).SetValidator(new AlterUserCommandPartValidator());
    }
}
