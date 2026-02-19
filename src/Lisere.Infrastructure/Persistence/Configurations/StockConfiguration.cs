using Lisere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisere.Infrastructure.Persistence.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.HasKey(s => new { s.ArticleId, s.Size });

        builder.Property(s => s.Size)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.HasOne(s => s.Article)
            .WithMany()
            .HasForeignKey(s => s.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
