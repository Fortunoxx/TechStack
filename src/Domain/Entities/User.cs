using TechStack.Domain.Common;

namespace TechStack.Domain.Entities;

public class User : BaseAuditableEntity
{
    public string Email { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Mobile { get; init; }
    public string Phone { get; init; }
}