namespace TechStack.Application.Users.Queries;

public record GetAllUsersQueryResult(IEnumerable<GetUserByIdQueryResult> Items);
