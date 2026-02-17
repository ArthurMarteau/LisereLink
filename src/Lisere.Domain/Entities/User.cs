using Lisere.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Lisere.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public ZoneType? AssignedZone { get; set; }

    // Audit fields (duplicated from BaseEntity — C# does not support multiple inheritance)
    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? ModifiedAt { get; set; }

    public string? ModifiedBy { get; set; }

    public bool IsDeleted { get; set; }
}
