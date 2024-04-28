using FluentValidation;

namespace TechStack.Application.Users.Commands;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator () => RuleFor(x => x.Id).GreaterThan(0);
}
