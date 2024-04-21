namespace TechStack.Domain.Entities;

using System.ComponentModel.DataAnnotations;
using TechStack.Domain.Common;

public class User : BaseAuditableEntity
{
    public string? AboutMe { get; init; }

    public int? Age { get; init; }

    [MaxLength(40)]
    public string? DisplayName { get; init; }

    public int? DownVotes { get; init; }

    [MaxLength(40)]
    public string? EmailHash { get; init; }

    public DateTime? LastAccessDate { get; init; }

    [MaxLength(100)]
    public string? Location { get; init; }

    public int? Reputation { get; init; }

    public int? UpVotes { get; init; }

    public int? Views { get; init; }

    [MaxLength(200)]
    public string? WebsiteUrl { get; init; }

    public int? AccountId { get; init; }
}