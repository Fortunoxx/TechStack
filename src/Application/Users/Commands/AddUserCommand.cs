namespace TechStack.Application.Users.Commands;

public record AddUserCommand(string Email, string FirstName, string LastName, string Mobile, string Phone);
