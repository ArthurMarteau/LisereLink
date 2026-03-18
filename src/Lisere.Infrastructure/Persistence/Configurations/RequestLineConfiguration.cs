using Lisere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lisere.Infrastructure.Persistence.Configurations;

public class RequestLineConfiguration : IEntityTypeConfiguration<RequestLine>
{
    public void Configure(EntityTypeBuilder<RequestLine> builder)
    {
        builder.HasKey(rl => rl.Id);

        builder.Property(rl => rl.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(rl => rl.ArticleName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(rl => rl.ArticleColorOrPrint)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(rl => rl.ArticleBarcode)
            .IsRequired()
            .HasMaxLength(20);

        var sizesConverter = new ValueConverter<List<string>, string>(
            v => string.Join(',', v),
            v => string.IsNullOrEmpty(v)
                ? new List<string>()
                : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());

        var sizesComparer = new ValueComparer<List<string>>(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        builder.Property(rl => rl.RequestedSizes)
            .HasConversion(sizesConverter, sizesComparer)
            .HasColumnType("nvarchar(200)");

        builder.Property(rl => rl.AlternativeColorOrPrint)
            .HasMaxLength(200);

        var altSizesConverter = new ValueConverter<List<string>?, string?>(
            v => v == null ? null : string.Join(',', v),
            v => string.IsNullOrEmpty(v)
                ? null
                : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());

        var altSizesComparer = new ValueComparer<List<string>?>(
            (c1, c2) => c1 == null ? c2 == null : c2 != null && c1.SequenceEqual(c2),
            c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c == null ? null : c.ToList());

        builder.Property(rl => rl.AlternativeSizes)
            .HasConversion(altSizesConverter, altSizesComparer)
            .HasColumnType("nvarchar(200)");

        builder.Property(rl => rl.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(rl => rl.ModifiedBy)
            .HasMaxLength(256);

        builder.HasQueryFilter(rl => !rl.IsDeleted);
    }
}
