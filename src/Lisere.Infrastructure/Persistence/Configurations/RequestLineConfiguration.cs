using Lisere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

        builder.Property(rl => rl.Size)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(rl => rl.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(rl => rl.ModifiedBy)
            .HasMaxLength(256);

        builder.HasQueryFilter(rl => !rl.IsDeleted);
    }
}
