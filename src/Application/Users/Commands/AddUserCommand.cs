namespace TechStack.Application.Users.Commands;

public record AddUserCommand(string? AboutMe, int? Age, string? DisplayName, int? DownVotes, string? EmailHash, DateTime? LastAccessDate, string? Location, int? Reputation, int? UpVotes, int? Views, string? WebsiteUrl, int? AccountId);
