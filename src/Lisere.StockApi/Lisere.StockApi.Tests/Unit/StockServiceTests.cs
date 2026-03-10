using Lisere.StockApi.Application.DTOs;
using Lisere.StockApi.Application.Exceptions;
using Lisere.StockApi.Application.Interfaces;
using Lisere.StockApi.Application.Services;
using Lisere.StockApi.Domain.Entities;
using Lisere.StockApi.Domain.Enums;
using Lisere.StockApi.Domain.Interfaces;
using NSubstitute;
using Xunit;

namespace Lisere.StockApi.Tests.Unit;

public class StockServiceTests
{
    private readonly IStockEntryRepository _stockEntryRepo;
    private readonly IStoreRepository _storeRepo;
    private readonly IArticleRepository _articleRepo;
    private readonly IWebhookNotifier _webhookNotifier;
    private readonly StockService _service;

    public StockServiceTests()
    {
        _stockEntryRepo = Substitute.For<IStockEntryRepository>();
        _storeRepo = Substitute.For<IStoreRepository>();
        _articleRepo = Substitute.For<IArticleRepository>();
        _webhookNotifier = Substitute.For<IWebhookNotifier>();
        _service = new StockService(_stockEntryRepo, _storeRepo, _articleRepo, _webhookNotifier);
    }

    // ── UpdateStockAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStockAsync_WhenQuantityIsNegative_ThrowsStockException()
    {
        var dto = new UpdateStockDto
        {
            ArticleId = Guid.NewGuid(),
            Size = Size.M,
            StoreId = "paris-2",
            NewQuantity = -1
        };

        await Assert.ThrowsAsync<StockException>(() => _service.UpdateStockAsync(dto));

        await _stockEntryRepo.DidNotReceive().UpsertAsync(Arg.Any<StockEntry>(), Arg.Any<CancellationToken>());
        await _webhookNotifier.DidNotReceive().NotifyStockUpdatedAsync(Arg.Any<Guid>(), Arg.Any<string>());
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

        _articleRepo.GetByIdAsync(articleId, Arg.Any<CancellationToken>()).Returns(article);
        _storeRepo.GetByCodeAsync("paris-2", Arg.Any<CancellationToken>()).Returns((Store?)null);
        _stockEntryRepo.UpsertAsync(Arg.Any<StockEntry>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var before = DateTime.UtcNow;

        var dto = new UpdateStockDto
        {
            ArticleId = articleId,
            Size = Size.M,
            StoreId = "paris-2",
            NewQuantity = 5
        };

        await _service.UpdateStockAsync(dto);

        await _stockEntryRepo.Received(1).UpsertAsync(
            Arg.Is<StockEntry>(e =>
                e.ArticleId == articleId &&
                e.Size == Size.M &&
                e.AvailableQuantity == 5 &&
                e.LastUpdatedAt >= before),
            Arg.Any<CancellationToken>());

        await _webhookNotifier.Received(1).NotifyStockUpdatedAsync(articleId, "paris-2");
    }

    // ── GetStockAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStockAsync_WhenEntriesExist_ReturnsOnlyEntriesForRequestedStore()
    {
        var articleId = Guid.NewGuid();
        const string storeId = "paris-2";

        var entries = new List<StockEntry>
        {
            new() { Id = Guid.NewGuid(), ArticleId = articleId, StoreId = storeId, Size = Size.S, AvailableQuantity = 2, StoreType = StoreType.Physical, LastUpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), ArticleId = articleId, StoreId = storeId, Size = Size.M, AvailableQuantity = 3, StoreType = StoreType.Physical, LastUpdatedAt = DateTime.UtcNow },
        };

        _stockEntryRepo.GetByArticleAsync(articleId, storeId, Arg.Any<CancellationToken>()).Returns(entries);

        var result = (await _service.GetStockAsync(articleId, storeId)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal(storeId, e.StoreId));
    }

    [Fact]
    public async Task GetStockAsync_WhenNoEntriesExist_ReturnsEmptyList()
    {
        var articleId = Guid.NewGuid();

        _stockEntryRepo.GetByArticleAsync(articleId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<StockEntry>());

        var result = await _service.GetStockAsync(articleId, "any-store");

        Assert.Empty(result);
    }
}
