namespace TechStack.Application.Users.Commands;

public record AlterUserCommand(int Id, AlterUserCommandPart User);