using Lisere.Domain.Entities;
using Lisere.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Lisere.Infrastructure.Persistence.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Barcode)
            .IsRequired()
            .HasMaxLength(13);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.ColorOrPrint)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Family)
            .HasConversion<string>()
            .HasMaxLength(3);

        var sizesConverter = new ValueConverter<List<Size>, string>(
            v => string.Join(',', v.Select(s => s.ToString())),
            v => string.IsNullOrEmpty(v)
                ? new List<Size>()
                : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                   .Select(s => Enum.Parse<Size>(s))
                   .ToList());

        var sizesComparer = new ValueComparer<List<Size>>(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        builder.Property(a => a.AvailableSizes)
            .HasConversion(sizesConverter, sizesComparer)
            .HasColumnType("nvarchar(200)");

        builder.Property(a => a.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.ModifiedBy)
            .HasMaxLength(256);

        builder.Property(a => a.Price)
            .HasPrecision(10, 2);

        builder.Property(a => a.ImageUrl)
            .HasMaxLength(500);

        builder.HasIndex(a => a.Barcode)
            .IsUnique();

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
