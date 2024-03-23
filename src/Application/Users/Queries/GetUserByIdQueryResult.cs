namespace TechStack.Application.Users.Queries;

public record GetUserByIdQueryResult(int Id, string? AboutMe, int? Age, string DisplayName, int DownVotes, string? EmailHash, DateTime LastAccessDate, string? Location, int Reputation, int UpVotes, int Views, string? WebsiteUrl, int? AccountId, DateTimeOffset? Created, string? CreatedBy, DateTimeOffset? LastModified, string? LastModifiedBy);
