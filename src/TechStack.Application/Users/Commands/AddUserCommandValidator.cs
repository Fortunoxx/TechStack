using FluentValidation;

namespace TechStack.Application.Users.Commands;

public class AddUserCommandValidator : AbstractValidator<AddUserCommand>
{
    public AddUserCommandValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty();
        RuleFor(x => x.DownVotes).NotEmpty();
        RuleFor(x => x.LastAccessDate).NotEmpty();
        RuleFor(x => x.Reputation).NotEmpty();
        RuleFor(x => x.UpVotes).NotEmpty();
        RuleFor(x => x.Views).NotEmpty();
    }
}
