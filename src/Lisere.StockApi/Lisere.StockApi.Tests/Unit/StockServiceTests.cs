using Lisere.StockApi.Application.DTOs;
using Lisere.StockApi.Application.Exceptions;
using Lisere.StockApi.Domain.Entities;
using Lisere.StockApi.Domain.Enums;
using NSubstitute;
using Xunit;

namespace Lisere.StockApi.Tests.Unit;

public class StockServiceTests : StockServiceTestBase
{
    // ── UpdateStockAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStockAsync_WhenQuantityIsNegative_ThrowsStockException()
    {
        var dto = new UpdateStockDto
        {
            ArticleId = Guid.NewGuid(),
            Size = Size.M,
            StoreId = "002",
            NewQuantity = -1
        };

        await Assert.ThrowsAsync<StockException>(() => Service.UpdateStockAsync(dto));

        await StockEntryRepo.DidNotReceive().UpsertAsync(Arg.Any<StockEntry>(), Arg.Any<CancellationToken>());
        await WebhookNotifier.DidNotReceive().NotifyStockUpdatedAsync(Arg.Any<Guid>(), Arg.Any<string>());
    }

    [Fact]
    public async Task UpdateStockAsync_WhenQuantityIsValid_CallsUpsertWithCorrectLastUpdatedAt()
    {
        var articleId = Guid.NewGuid();
        var article = new Article
        {
            Id = articleId,
            Barcode = "1234567890123",
            Name = "Test Article",
            ColorOrPrint = "Noir",
            Family = ClothingFamily.TSH,
            AvailableSizes = [Size.M],
            LastUpdatedAt = DateTime.UtcNow
        };

        ArticleRepo.GetByIdAsync(articleId, Arg.Any<CancellationToken>()).Returns(article);
        StoreRepo.GetByCodeAsync("002", Arg.Any<CancellationToken>()).Returns((Store?)null);
        StockEntryRepo.UpsertAsync(Arg.Any<StockEntry>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var before = DateTime.UtcNow;

        var dto = new UpdateStockDto
        {
            ArticleId = articleId,
            Size = Size.M,
            StoreId = "002",
            NewQuantity = 5
        };

        await Service.UpdateStockAsync(dto);

        await StockEntryRepo.Received(1).UpsertAsync(
            Arg.Is<StockEntry>(e =>
                e.ArticleId == articleId &&
                e.Size == Size.M &&
                e.AvailableQuantity == 5 &&
                e.LastUpdatedAt >= before),
            Arg.Any<CancellationToken>());

        await WebhookNotifier.Received(1).NotifyStockUpdatedAsync(articleId, "002");
    }

    // ── GetStockAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStockAsync_WhenEntriesExist_ReturnsOnlyEntriesForRequestedStore()
    {
        var articleId = Guid.NewGuid();
        const string storeId = "002";

        var entries = new List<StockEntry>
        {
            new() { Id = Guid.NewGuid(), ArticleId = articleId, StoreId = storeId, Size = Size.S, AvailableQuantity = 2, StoreType = StoreType.Physical, LastUpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), ArticleId = articleId, StoreId = storeId, Size = Size.M, AvailableQuantity = 3, StoreType = StoreType.Physical, LastUpdatedAt = DateTime.UtcNow },
        };

        StockEntryRepo.GetByArticleAsync(articleId, storeId, Arg.Any<CancellationToken>()).Returns(entries);

        var result = (await Service.GetStockAsync(articleId, storeId)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal(storeId, e.StoreId));
    }

    [Fact]
    public async Task GetStockAsync_WhenNoEntriesExist_ReturnsEmptyList()
    {
        var articleId = Guid.NewGuid();

        StockEntryRepo.GetByArticleAsync(articleId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<StockEntry>());

        var result = await Service.GetStockAsync(articleId, "any-store");

        Assert.Empty(result);
    }

    // ── GetAllArticlesWithStockAsync ──────────────────────────────────────

    [Fact]
    public async Task GetAllArticlesWithStockAsync_WhenArticleNotFound_UsesEmptyStringFallbacks()
    {
        var articleId = Guid.NewGuid();
        var entry = new StockEntry
        {
            Id                = Guid.NewGuid(),
            ArticleId         = articleId,
            StoreId           = "002",
            Size              = Size.M,
            AvailableQuantity = 3,
            StoreType         = StoreType.Physical,
            LastUpdatedAt     = DateTime.UtcNow,
        };

        StockEntryRepo
            .GetByStoreAsync("002", Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(((IEnumerable<StockEntry>)[entry], 1));

        ArticleRepo
            .GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Article>());

        var result = await Service.GetAllArticlesWithStockAsync("002", 1, 20);

        var dto = result.Items.Single();
        Assert.Equal(string.Empty, dto.Barcode);
        Assert.Equal(string.Empty, dto.Name);
        Assert.Equal(string.Empty, dto.Family);
        Assert.Equal(string.Empty, dto.ColorOrPrint);
    }

    [Fact]
    public async Task GetAllArticlesWithStockAsync_ReturnsOnlyEntriesForRequestedStore()
    {
        var articleId = Guid.NewGuid();
        var entries = new List<StockEntry>
        {
            new() { Id = Guid.NewGuid(), ArticleId = articleId, StoreId = "002", Size = Size.S, AvailableQuantity = 1, StoreType = StoreType.Physical, LastUpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), ArticleId = articleId, StoreId = "002", Size = Size.M, AvailableQuantity = 2, StoreType = StoreType.Physical, LastUpdatedAt = DateTime.UtcNow },
        };

        var article = new Article
        {
            Id             = articleId,
            Barcode        = "1234567890123",
            Name           = "Test",
            ColorOrPrint   = "Noir",
            Family         = ClothingFamily.TSH,
            AvailableSizes = [Size.S, Size.M],
            LastUpdatedAt  = DateTime.UtcNow,
        };

        StockEntryRepo
            .GetByStoreAsync("002", Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(((IEnumerable<StockEntry>)entries, 2));

        ArticleRepo
            .GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Article> { article });

        var result = await Service.GetAllArticlesWithStockAsync("002", 1, 20);

        Assert.Equal(2, result.TotalCount);
        var dto = result.Items.Single();
        Assert.Equal(articleId, dto.ArticleId);
        Assert.All(dto.Stock, s => Assert.Equal("002", s.StoreId));
    }

    [Fact]
    public async Task GetAllArticlesWithStockAsync_WithDifferentStoreId_ReturnsEmpty()
    {
        // Tue le mutant storeId passé au repo muté vers une autre valeur (ligne 83)
        StockEntryRepo
            .GetByStoreAsync("004", Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(((IEnumerable<StockEntry>)[], 0));

        ArticleRepo
            .GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Article>());

        var result = await Service.GetAllArticlesWithStockAsync("004", 1, 20);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    // Note : les mutants "remove left" sur `article?.Barcode ?? string.Empty` (lignes 64-67 de StockService)
    // sont des équivalents structurels : Article.Barcode est string non-nullable (défaut string.Empty),
    // donc le ?? string.Empty est redondant quand article est non-null.
    // Le test WhenArticleNotFound_UsesEmptyStringFallbacks couvre le cas article==null.

    [Fact]
    public async Task GetAllArticlesWithStockAsync_WithPageSizeBelowMax_PreservesPageSize()
    {
        // Tue le mutant Math.Min → Math.Max (ligne 46) : Min(20,50)=20 mais Max(20,50)=50
        StockEntryRepo
            .GetByStoreAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(((IEnumerable<StockEntry>)[], 0));
        ArticleRepo
            .GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Article>());

        await Service.GetAllArticlesWithStockAsync("002", 1, 20);

        await StockEntryRepo.Received(1).GetByStoreAsync("002", 1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllArticlesWithStockAsync_CallsRepositoryWithExactStoreId()
    {
        // Tue la mutation storeId passé au repo — mock pinned sur "002" avec Arg.Is précis
        StockEntryRepo
            .GetByStoreAsync(Arg.Is<string>(s => s == "002"), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(((IEnumerable<StockEntry>)[], 0));
        ArticleRepo
            .GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Article>());

        await Service.GetAllArticlesWithStockAsync("002", 1, 20);

        await StockEntryRepo.Received(1).GetByStoreAsync("002", Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllArticlesWithStockAsync_WebhookNotCalledOnRead()
    {
        StockEntryRepo
            .GetByStoreAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(((IEnumerable<StockEntry>)[], 0));

        ArticleRepo
            .GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Article>());

        await Service.GetAllArticlesWithStockAsync("any-store", 1, 20);

        await WebhookNotifier
            .DidNotReceive()
            .NotifyStockUpdatedAsync(Arg.Any<Guid>(), Arg.Any<string>());
    }
}
