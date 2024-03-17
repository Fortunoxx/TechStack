namespace TechStack.Application.Users.Queries;

public record GetUserByIdQueryResult(int Id, string Email, string FirstName, string LastName, string Mobile, string Phone, DateTimeOffset Created, string CreatedBy, DateTimeOffset? LastModified, string LastModifiedBy);