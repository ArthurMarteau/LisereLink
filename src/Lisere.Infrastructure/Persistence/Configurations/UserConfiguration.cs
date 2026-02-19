using Lisere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisere.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.AssignedZone)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.ModifiedBy)
            .HasMaxLength(256);

        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
