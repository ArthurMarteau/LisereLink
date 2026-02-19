using Lisere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisere.Infrastructure.Persistence.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.Zone)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.ModifiedBy)
            .HasMaxLength(256);

        builder.HasOne(r => r.Seller)
            .WithMany()
            .HasForeignKey(r => r.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Stockist)
            .WithMany()
            .HasForeignKey(r => r.StockistId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Lines)
            .WithOne(l => l.Request)
            .HasForeignKey(l => l.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.Zone);
        builder.HasIndex(r => r.CreatedAt);

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
