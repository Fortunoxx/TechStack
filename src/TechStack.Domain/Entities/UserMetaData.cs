namespace TechStack.Domain.Entities;

using TechStack.Domain.Common;

public class UserMetaData : BaseAuditableEntity
{
    public string? MetaKey { get; init; }
    public string? MetaValue { get; init; }
    public int UserId { get; init; }
    public User? User { get; init; }
}