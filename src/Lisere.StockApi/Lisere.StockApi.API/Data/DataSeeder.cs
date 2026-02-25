using Lisere.Domain.Entities;
using Lisere.Domain.Enums;
using Lisere.StockApi.Domain.Entities;
using Lisere.StockApi.Domain.Enums;
using Lisere.StockApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Lisere.StockApi.API.Data;

/// <summary>
/// Seeder de données de développement — exécuté uniquement en IsDevelopment.
/// Crée 3 magasins, 20 articles (toutes ClothingFamily), et les StockEntries associées.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(StockApiDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Stores.AnyAsync())
            return; // Déjà seedé

        // ── Stores ──────────────────────────────────────────────────────────
        var stores = new[]
        {
            new Store { Id = Guid.NewGuid(), Code = "002",    Name = "Paris 2",      Type = StoreType.Physical },
            new Store { Id = Guid.NewGuid(), Code = "004", Name = "Paris 4",   Type = StoreType.Physical },
            new Store { Id = Guid.NewGuid(), Code = "online",         Name = "Online",            Type = StoreType.Online  },
        };
        await context.Stores.AddRangeAsync(stores);
        await context.SaveChangesAsync();

        // ── Articles — 20 articles couvrant toutes les ClothingFamily ───────
        var articles = new List<Article>
        {
            // COA – Manteaux
            new() { Id = Guid.NewGuid(), Barcode = "3400936123450", Family = ClothingFamily.COA, Name = "Manteau Mathilde", ColorOrPrint = "Camel",          AvailableSizes = [Size.XS, Size.S, Size.M, Size.L, Size.XL] },
            new() { Id = Guid.NewGuid(), Barcode = "3400936123451", Family = ClothingFamily.COA, Name = "Trench Colette",   ColorOrPrint = "Beige",          AvailableSizes = [Size.S, Size.M, Size.L] },
            // JAC – Vestes
            new() { Id = Guid.NewGuid(), Barcode = "3400936123452", Family = ClothingFamily.JAC, Name = "Veste Apolline",   ColorOrPrint = "Noir",           AvailableSizes = [Size.XS, Size.S, Size.M, Size.L] },
            // TSH – T-shirts
            new() { Id = Guid.NewGuid(), Barcode = "3400936123453", Family = ClothingFamily.TSH, Name = "T-Shirt Léonie",   ColorOrPrint = "Blanc",          AvailableSizes = [Size.XS, Size.S, Size.M, Size.L, Size.XL, Size.XXL] },
            new() { Id = Guid.NewGuid(), Barcode = "3400936123454", Family = ClothingFamily.TSH, Name = "T-Shirt Léonie",   ColorOrPrint = "Marinière",      AvailableSizes = [Size.S, Size.M, Size.L] },
            // SWE – Pulls
            new() { Id = Guid.NewGuid(), Barcode = "3400936123455", Family = ClothingFamily.SWE, Name = "Pull Céleste",     ColorOrPrint = "Marine",         AvailableSizes = [Size.XS, Size.S, Size.M, Size.L] },
            // VES – Gilets
            new() { Id = Guid.NewGuid(), Barcode = "3400936123456", Family = ClothingFamily.VES, Name = "Gilet Honorine",   ColorOrPrint = "Crème",          AvailableSizes = [Size.S, Size.M, Size.L, Size.XL] },
            // JEA – Jeans
            new() { Id = Guid.NewGuid(), Barcode = "3400936123457", Family = ClothingFamily.JEA, Name = "Jean Victoire",    ColorOrPrint = "Bleu Brut",      AvailableSizes = [Size.XS, Size.S, Size.M, Size.L, Size.XL] },
            // PAN – Pantalons
            new() { Id = Guid.NewGuid(), Barcode = "3400936123458", Family = ClothingFamily.PAN, Name = "Pantalon Simone",  ColorOrPrint = "Kaki",           AvailableSizes = [Size.S, Size.M, Size.L, Size.XL] },
            // SHO – Shorts
            new() { Id = Guid.NewGuid(), Barcode = "3400936123459", Family = ClothingFamily.SHO, Name = "Short Adèle",      ColorOrPrint = "Blanc",          AvailableSizes = [Size.S, Size.M, Size.L] },
            // SKI – Jupes
            new() { Id = Guid.NewGuid(), Barcode = "3400936123460", Family = ClothingFamily.SKI, Name = "Jupe Rosalie",     ColorOrPrint = "Fleurs Ocre",    AvailableSizes = [Size.XS, Size.S, Size.M, Size.L] },
            // DRE – Robes
            new() { Id = Guid.NewGuid(), Barcode = "3400936123461", Family = ClothingFamily.DRE, Name = "Robe Emma",        ColorOrPrint = "Rouge",          AvailableSizes = [Size.XS, Size.S, Size.M, Size.L] },
            new() { Id = Guid.NewGuid(), Barcode = "3400936123462", Family = ClothingFamily.DRE, Name = "Robe Emma",        ColorOrPrint = "Noir",           AvailableSizes = [Size.S, Size.M, Size.L, Size.XL] },
            // SHI – Chemises
            new() { Id = Guid.NewGuid(), Barcode = "3400936123463", Family = ClothingFamily.SHI, Name = "Chemise Margaux",  ColorOrPrint = "Bleu Ciel",      AvailableSizes = [Size.S, Size.M, Size.L, Size.XL] },
            // BLO – Blouses
            new() { Id = Guid.NewGuid(), Barcode = "3400936123464", Family = ClothingFamily.BLO, Name = "Blouse Hortense",  ColorOrPrint = "Ivoire",         AvailableSizes = [Size.XS, Size.S, Size.M, Size.L] },
            // SHE – Chaussures
            new() { Id = Guid.NewGuid(), Barcode = "3400936123465", Family = ClothingFamily.SHE, Name = "Sneakers Ninon",   ColorOrPrint = "Blanc",          AvailableSizes = [Size.S, Size.M, Size.L, Size.XL] },
            // BEL – Ceintures
            new() { Id = Guid.NewGuid(), Barcode = "3400936123466", Family = ClothingFamily.BEL, Name = "Ceinture Diane",   ColorOrPrint = "Cognac",         AvailableSizes = [Size.OneSize] },
            // BAG – Sacs
            new() { Id = Guid.NewGuid(), Barcode = "3400936123467", Family = ClothingFamily.BAG, Name = "Sac Célimène",     ColorOrPrint = "Écru",           AvailableSizes = [Size.OneSize] },
            // JEW – Bijoux
            new() { Id = Guid.NewGuid(), Barcode = "3400936123468", Family = ClothingFamily.JEW, Name = "Collier Aurore",   ColorOrPrint = "Or",             AvailableSizes = [Size.OneSize] },
            new() { Id = Guid.NewGuid(), Barcode = "3400936123469", Family = ClothingFamily.JEW, Name = "Boucles Céleste",  ColorOrPrint = "Argent",         AvailableSizes = [Size.OneSize] },
        };

        // Set audit fields required by BaseEntity
        var now = DateTime.UtcNow;
        foreach (var a in articles)
        {
            a.CreatedAt = now;
            a.CreatedBy = "seeder";
        }

        await context.Articles.AddRangeAsync(articles);
        await context.SaveChangesAsync();

        // ── StockEntries ─────────────────────────────────────────────────────
        // Quantités Physical : variées par taille, certaines à 0
        // Quantités Online  : toujours >= Physical
        var stockEntries = new List<StockEntry>();
        var physicalStores = stores.Where(s => s.Type == StoreType.Physical).ToArray();
        var onlineStore = stores.First(s => s.Type == StoreType.Online);

        // Quantités de base par taille (index correspond à l'ordre des tailles dans AvailableSizes)
        int[] physParis2Qty  = [3, 2, 1, 0, 2, 2]; // certaines tailles à 0
        int[] physParis4Qty   = [2, 1, 0, 3, 1, 1];
        int[] onlineQty     = [8, 6, 5, 4, 7, 6]; // toujours supérieur au Physical

        foreach (var article in articles)
        {
            for (int i = 0; i < article.AvailableSizes.Count; i++)
            {
                var size = article.AvailableSizes[i];

                // Paris 2
                stockEntries.Add(new StockEntry
                {
                    Id = Guid.NewGuid(),
                    ArticleId = article.Id,
                    Size = size,
                    StoreId = physicalStores[0].Code,
                    StoreType = StoreType.Physical,
                    AvailableQuantity = physParis2Qty[i % physParis2Qty.Length],
                    LastUpdatedAt = now
                });

                // Paris 4
                stockEntries.Add(new StockEntry
                {
                    Id = Guid.NewGuid(),
                    ArticleId = article.Id,
                    Size = size,
                    StoreId = physicalStores[1].Code,
                    StoreType = StoreType.Physical,
                    AvailableQuantity = physParis4Qty[i % physParis4Qty.Length],
                    LastUpdatedAt = now
                });

                // Online
                stockEntries.Add(new StockEntry
                {
                    Id = Guid.NewGuid(),
                    ArticleId = article.Id,
                    Size = size,
                    StoreId = onlineStore.Code,
                    StoreType = StoreType.Online,
                    AvailableQuantity = onlineQty[i % onlineQty.Length],
                    LastUpdatedAt = now
                });
            }
        }

        await context.StockEntries.AddRangeAsync(stockEntries);
        await context.SaveChangesAsync();
    }
}
