using System.ComponentModel.DataAnnotations;

namespace TechStack.Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTimeOffset Created { get; set; }

    [MaxLength(8)]
    public string? CreatedBy { get; set; }

    public DateTimeOffset? LastModified { get; set; }

    [MaxLength(8)]
    public string? LastModifiedBy { get; set; }
}
