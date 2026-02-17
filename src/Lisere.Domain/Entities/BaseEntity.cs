namespace Lisere.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }

    public bool IsDeleted { get; set; }
}
