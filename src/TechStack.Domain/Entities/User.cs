namespace TechStack.Domain.Entities;

using TechStack.Domain.Common;

public class User : BaseAuditableEntity
{
    public string? AboutMe { get; init; }
    public int? Age { get; init; }
    public string? DisplayName { get; init; }
    public int? DownVotes { get; init; }
    public string? EmailHash { get; init; }
    public DateTime? LastAccessDate { get; init; }
    public string? Location { get; init; }
    public int? Reputation { get; init; }
    public int? UpVotes { get; init; }
    public int? Views { get; init; }
    public string? WebsiteUrl { get; init; }
    public int? AccountId { get; init; }
    public ICollection<UserMetaData>? MetaData { get; init; }
}
