using Lisere.Domain.Entities;
using Lisere.Domain.Enums;
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
            .HasMaxLength(20);

        builder.Property(rl => rl.ColorOrPrint)
            .IsRequired()
            .HasMaxLength(100);

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

        builder.Property(rl => rl.RequestedSizes)
            .HasConversion(sizesConverter, sizesComparer)
            .HasColumnType("nvarchar(200)");

        builder.Property(rl => rl.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(rl => rl.ModifiedBy)
            .HasMaxLength(256);

        builder.HasOne(rl => rl.Article)
            .WithMany()
            .HasForeignKey(rl => rl.ArticleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(rl => !rl.IsDeleted);
    }
}
