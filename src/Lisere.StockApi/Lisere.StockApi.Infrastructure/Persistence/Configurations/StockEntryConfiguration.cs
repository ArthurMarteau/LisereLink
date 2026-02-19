using Lisere.StockApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisere.StockApi.Infrastructure.Persistence.Configurations;

public class StockEntryConfiguration : IEntityTypeConfiguration<StockEntry>
{
    public void Configure(EntityTypeBuilder<StockEntry> builder)
    {
        builder.HasKey(se => se.Id);

        builder.Property(se => se.Size)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(se => se.StoreType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(se => se.StoreId)
            .IsRequired()
            .HasMaxLength(100);

        // Contrainte d'unicité métier : un article/taille/magasin = une seule entrée de stock
        builder.HasIndex(se => new { se.ArticleId, se.Size, se.StoreId })
            .IsUnique();

        builder.HasIndex(se => se.StoreId);
    }
}
