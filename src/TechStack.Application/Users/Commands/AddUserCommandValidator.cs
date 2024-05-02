using FluentValidation;

namespace TechStack.Application.Users.Commands;

public class AddUserCommandValidator : AbstractValidator<AddUserCommand>
{
    public AddUserCommandValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty();
        RuleFor(x => x.DownVotes).NotNull().GreaterThanOrEqualTo(0);
        RuleFor(x => x.LastAccessDate).NotEmpty();
        RuleFor(x => x.Reputation).NotNull().GreaterThanOrEqualTo(0);
        RuleFor(x => x.UpVotes).NotNull().GreaterThanOrEqualTo(0);
        RuleFor(x => x.Views).NotNull().GreaterThanOrEqualTo(0);
    }
}
