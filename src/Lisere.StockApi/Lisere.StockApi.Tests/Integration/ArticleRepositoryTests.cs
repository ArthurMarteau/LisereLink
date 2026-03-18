using Lisere.StockApi.Domain.Entities;
using Lisere.StockApi.Domain.Enums;
using Lisere.StockApi.Infrastructure.Persistence;
using Lisere.StockApi.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Lisere.StockApi.Tests.Integration;

public class ArticleRepositoryTests
{
    private static StockApiDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<StockApiDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static Article BuildArticle(ClothingFamily family, string name, string barcode) => new()
    {
        Id            = Guid.NewGuid(),
        Family        = family,
        Name          = name,
        Barcode       = barcode,
        ColorOrPrint  = "Noir",
        LastUpdatedAt = DateTime.UtcNow,
    };

    // ── GetAllAsync — ordre ────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsArticlesOrderedByFamilyThenName()
    {
        // Tue OrderBy→OrderByDescending ET ThenBy→ThenByDescending (lignes 40-43)
        // COA=0, TSH=2 dans l'enum → COA < TSH ; "Alpha" < "Zèbre" lexicographique
        await using var context = CreateContext();
        var repo = new ArticleRepository(context);

        var tsh_zebre = BuildArticle(ClothingFamily.TSH, "Zèbre",  "001");
        var coa_alpha = BuildArticle(ClothingFamily.COA, "Alpha",  "002");
        var tsh_alpha = BuildArticle(ClothingFamily.TSH, "Alpha",  "003");

        context.Articles.AddRange(tsh_zebre, coa_alpha, tsh_alpha);
        await context.SaveChangesAsync();

        var (items, totalCount) = await repo.GetAllAsync(1, 10);
        var list = items.ToList();

        Assert.Equal(3, totalCount);
        Assert.Equal(3, list.Count);
        // Ordre attendu : COA/Alpha, TSH/Alpha, TSH/Zèbre
        Assert.Equal(coa_alpha.Id, list[0].Id);
        Assert.Equal(tsh_alpha.Id, list[1].Id);
        Assert.Equal(tsh_zebre.Id, list[2].Id);
    }

    // ── GetAllAsync — pagination ───────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_Page2_SkipsFirstPageCorrectly()
    {
        // Tue le mutant (page - 1) → (page + 1) ou page (ligne 49)
        await using var context = CreateContext();
        var repo = new ArticleRepository(context);

        // Ordre déterministe après OrderBy(Family).ThenBy(Name) :
        // 0: COA/Article1, 1: COA/Article2, 2: TSH/Article1, 3: TSH/Article2
        var coa1 = BuildArticle(ClothingFamily.COA, "Article1", "C01");
        var coa2 = BuildArticle(ClothingFamily.COA, "Article2", "C02");
        var tsh1 = BuildArticle(ClothingFamily.TSH, "Article1", "T01");
        var tsh2 = BuildArticle(ClothingFamily.TSH, "Article2", "T02");

        context.Articles.AddRange(coa1, coa2, tsh1, tsh2);
        await context.SaveChangesAsync();

        var (items, totalCount) = await repo.GetAllAsync(page: 2, pageSize: 2);
        var list = items.ToList();

        Assert.Equal(4, totalCount);
        Assert.Equal(2, list.Count);
        // Page 2 = skip (2-1)*2=2, take 2 → articles 3 et 4 : TSH/Article1 et TSH/Article2
        Assert.Equal(tsh1.Id, list[0].Id);
        Assert.Equal(tsh2.Id, list[1].Id);
    }
}
