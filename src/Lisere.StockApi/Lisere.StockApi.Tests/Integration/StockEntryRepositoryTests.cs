using Lisere.StockApi.Domain.Entities;
using Lisere.StockApi.Domain.Enums;
using Lisere.StockApi.Infrastructure.Persistence;
using Lisere.StockApi.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Lisere.StockApi.Tests.Integration;

public class StockEntryRepositoryTests
{
    private static StockApiDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<StockApiDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static StockEntry BuildEntry(
        Guid articleId,
        string storeId,
        Size size,
        int quantity = 1) => new()
    {
        Id                = Guid.NewGuid(),
        ArticleId         = articleId,
        StoreId           = storeId,
        Size              = size,
        AvailableQuantity = quantity,
        StoreType         = StoreType.Physical,
        LastUpdatedAt     = DateTime.UtcNow,
    };

    // ── GetByArticleAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetByArticleAsync_ReturnsOnlyMatchingArticleAndStore()
    {
        await using var context = CreateContext();
        var repo = new StockEntryRepository(context);

        var targetArticleId = Guid.NewGuid();
        const string targetStore = "002";
        const string otherStore  = "004";

        context.StockEntries.AddRange(
            BuildEntry(targetArticleId, targetStore, Size.S),
            BuildEntry(targetArticleId, targetStore, Size.M),
            BuildEntry(targetArticleId, otherStore,  Size.L));
        await context.SaveChangesAsync();

        var result = (await repo.GetByArticleAsync(targetArticleId, targetStore)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal(targetArticleId, e.ArticleId));
        Assert.All(result, e => Assert.Equal(targetStore, e.StoreId));
    }

    [Fact]
    public async Task GetByArticleAsync_ReturnsEntriesOrderedBySize()
    {
        await using var context = CreateContext();
        var repo = new StockEntryRepository(context);

        var articleId = Guid.NewGuid();
        const string storeId = "002";

        context.StockEntries.AddRange(
            BuildEntry(articleId, storeId, Size.L),
            BuildEntry(articleId, storeId, Size.S),
            BuildEntry(articleId, storeId, Size.M));
        await context.SaveChangesAsync();

        var result = (await repo.GetByArticleAsync(articleId, storeId)).ToList();

        Assert.Equal(3, result.Count);
        Assert.Equal(Size.S, result[0].Size);
        Assert.Equal(Size.M, result[1].Size);
        Assert.Equal(Size.L, result[2].Size);
    }

    // ── GetByStoreAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetByStoreAsync_ReturnsOnlyEntriesForTargetStore()
    {
        await using var context = CreateContext();
        var repo = new StockEntryRepository(context);

        var articleId   = Guid.NewGuid();
        const string targetStore = "002";
        const string otherStore  = "004";

        context.StockEntries.AddRange(
            BuildEntry(articleId, targetStore, Size.S),
            BuildEntry(articleId, targetStore, Size.M),
            BuildEntry(articleId, otherStore,  Size.L));
        await context.SaveChangesAsync();

        var (items, totalCount) = await repo.GetByStoreAsync(targetStore, 1, 10);

        Assert.Equal(2, totalCount);
        Assert.All(items, e => Assert.Equal(targetStore, e.StoreId));
    }

    [Fact]
    public async Task GetByStoreAsync_PaginationSkipsCorrectly()
    {
        await using var context = CreateContext();
        var repo = new StockEntryRepository(context);

        const string storeId = "002";
        var entries = Enumerable.Range(1, 5)
            .Select(_ => BuildEntry(Guid.NewGuid(), storeId, Size.S))
            .ToList();
        context.StockEntries.AddRange(entries);
        await context.SaveChangesAsync();

        var (page1Items, total) = await repo.GetByStoreAsync(storeId, 1, 2);
        var (page2Items, _)    = await repo.GetByStoreAsync(storeId, 2, 2);

        var page1Ids = page1Items.Select(e => e.Id).ToHashSet();
        var page2Ids = page2Items.Select(e => e.Id).ToHashSet();

        Assert.Equal(5, total);
        Assert.Equal(2, page1Ids.Count);
        Assert.Equal(2, page2Ids.Count);
        Assert.Empty(page1Ids.Intersect(page2Ids));
    }

    [Fact]
    public async Task GetByStoreAsync_CapsPageSizeAt50()
    {
        await using var context = CreateContext();
        var repo = new StockEntryRepository(context);

        const string storeId = "002";
        var entries = Enumerable.Range(1, 60)
            .Select(_ => BuildEntry(Guid.NewGuid(), storeId, Size.S))
            .ToList();
        context.StockEntries.AddRange(entries);
        await context.SaveChangesAsync();

        var (items, totalCount) = await repo.GetByStoreAsync(storeId, 1, 999);

        Assert.Equal(60, totalCount);
        Assert.Equal(50, items.Count());
    }

    [Fact]
    public async Task GetByStoreAsync_ReturnsEntriesOrderedByArticleIdThenSize()
    {
        // Tue OrderBy→OrderByDescending ET ThenBy→ThenByDescending (ligne 51)
        await using var context = CreateContext();
        var repo = new StockEntryRepository(context);

        // Guids déterministes : A < B selon la comparaison lexicographique
        var articleIdA = Guid.Parse("10000000-0000-0000-0000-000000000000");
        var articleIdB = Guid.Parse("20000000-0000-0000-0000-000000000000");
        const string storeId = "002";

        context.StockEntries.AddRange(
            BuildEntry(articleIdB, storeId, Size.M),
            BuildEntry(articleIdA, storeId, Size.S),
            BuildEntry(articleIdA, storeId, Size.M),
            BuildEntry(articleIdB, storeId, Size.S));
        await context.SaveChangesAsync();

        var (items, _) = await repo.GetByStoreAsync(storeId, 1, 10);
        var list = items.ToList();

        // OrderBy(ArticleId) asc, ThenBy(Size) asc → A/S, A/M, B/S, B/M
        Assert.Equal(4, list.Count);
        Assert.Equal(articleIdA, list[0].ArticleId); Assert.Equal(Size.S, list[0].Size);
        Assert.Equal(articleIdA, list[1].ArticleId); Assert.Equal(Size.M, list[1].Size);
        Assert.Equal(articleIdB, list[2].ArticleId); Assert.Equal(Size.S, list[2].Size);
        Assert.Equal(articleIdB, list[3].ArticleId); Assert.Equal(Size.M, list[3].Size);
    }

    // ── UpsertAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task UpsertAsync_WhenEntryDoesNotExist_InsertsNewEntry()
    {
        await using var context = CreateContext();
        var repo = new StockEntryRepository(context);

        var entry = BuildEntry(Guid.NewGuid(), "002", Size.M, quantity: 5);

        await repo.UpsertAsync(entry);

        var stored = await context.StockEntries.FindAsync(entry.Id);
        Assert.NotNull(stored);
        Assert.Equal(5, stored.AvailableQuantity);
        Assert.Equal(1, await context.StockEntries.CountAsync());
    }

    [Fact]
    public async Task UpsertAsync_WhenEntryExists_UpdatesQuantityAndTimestamp()
    {
        await using var context = CreateContext();
        var repo = new StockEntryRepository(context);

        var articleId = Guid.NewGuid();
        var existing  = BuildEntry(articleId, "002", Size.M, quantity: 3);
        existing.LastUpdatedAt = DateTime.UtcNow.AddHours(-1);
        context.StockEntries.Add(existing);
        await context.SaveChangesAsync();

        var before  = DateTime.UtcNow;
        var updated = BuildEntry(articleId, "002", Size.M, quantity: 10);

        await repo.UpsertAsync(updated);

        Assert.Equal(1, await context.StockEntries.CountAsync());
        var stored = await context.StockEntries.FirstAsync();
        Assert.Equal(10, stored.AvailableQuantity);
        Assert.True(stored.LastUpdatedAt >= before);
    }

    [Fact]
    public async Task UpsertAsync_WhenOnlyArticleIdMatches_CreatesNewEntry()
    {
        // Tue le mutant && → || dans la condition de recherche (ligne 69)
        await using var context = CreateContext();
        var repo = new StockEntryRepository(context);

        var articleId = Guid.NewGuid();
        var existing = BuildEntry(articleId, "002", Size.S, quantity: 3);
        context.StockEntries.Add(existing);
        await context.SaveChangesAsync();

        // ArticleId correspond mais Size et StoreId diffèrent → doit insérer
        var newEntry = BuildEntry(articleId, "004", Size.M, quantity: 7);
        await repo.UpsertAsync(newEntry);

        Assert.Equal(2, await context.StockEntries.CountAsync());
    }

    [Fact]
    public async Task UpsertAsync_WhenArticleAndSizeMatchButNotStore_CreatesNewEntry()
    {
        // Tue le mutant && → || : ArticleId+Size correspondent mais StoreId diffère → insert
        await using var context = CreateContext();
        var repo = new StockEntryRepository(context);

        var articleId = Guid.NewGuid();
        var existing = BuildEntry(articleId, "002", Size.S, quantity: 5);
        context.StockEntries.Add(existing);
        await context.SaveChangesAsync();

        var newEntry = BuildEntry(articleId, "004", Size.S, quantity: 8);
        await repo.UpsertAsync(newEntry);

        Assert.Equal(2, await context.StockEntries.CountAsync());
    }
}
