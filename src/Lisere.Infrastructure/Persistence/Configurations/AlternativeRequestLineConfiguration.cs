using System.Text.Json;
using Lisere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lisere.Infrastructure.Persistence.Configurations;

public class AlternativeRequestLineConfiguration : IEntityTypeConfiguration<AlternativeRequestLine>
{
    public void Configure(EntityTypeBuilder<AlternativeRequestLine> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(a => a.ArticleName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.ArticleColorOrPrint)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.ArticleBarcode)
            .IsRequired()
            .HasMaxLength(20);

        var sizesConverter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

        var sizesComparer = new ValueComparer<List<string>>(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        builder.Property(a => a.RequestedSizes)
            .HasConversion(sizesConverter, sizesComparer)
            .HasColumnType("nvarchar(500)");

        builder.Property(a => a.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.ModifiedBy)
            .HasMaxLength(256);

        builder.HasIndex(a => a.RequestId);

        builder.HasQueryFilter(a => !a.IsDeleted);

        builder.HasOne(a => a.Request)
            .WithMany(r => r.AlternativeLines)
            .HasForeignKey(a => a.RequestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
