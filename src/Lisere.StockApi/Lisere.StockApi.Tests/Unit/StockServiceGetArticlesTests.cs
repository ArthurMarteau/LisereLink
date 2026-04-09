using Lisere.StockApi.Domain.Entities;
using Lisere.StockApi.Domain.Enums;
using NSubstitute;
using Xunit;

namespace Lisere.StockApi.Tests.Unit;

public class StockServiceGetArticlesTests : StockServiceTestBase
{
    // ── GetArticlesAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetArticlesAsync_ReturnsMappedPagedResult()
    {
        var articles = new List<Article>
        {
            BuildArticle("1234567890001"),
            BuildArticle("1234567890002"),
        };

        ArticleRepo.GetAllAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns((articles.AsEnumerable(), 2));

        var result = await Service.GetArticlesAsync(1, 20);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
    }

    [Fact]
    public async Task GetArticlesAsync_CapsPageSizeAt50()
    {
        ArticleRepo.GetAllAsync(1, 50, Arg.Any<CancellationToken>())
            .Returns((Enumerable.Empty<Article>(), 0));

        await Service.GetArticlesAsync(1, 999);

        await ArticleRepo.Received(1).GetAllAsync(1, 50, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetArticlesAsync_ClampPageToMinimumOfOne()
    {
        ArticleRepo.GetAllAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns((Enumerable.Empty<Article>(), 0));

        await Service.GetArticlesAsync(0, 20);

        await ArticleRepo.Received(1).GetAllAsync(1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetArticlesAsync_MapsArticleFieldsCorrectly()
    {
        var article = BuildArticle("9999999999999");
        ArticleRepo.GetAllAsync(1, 10, Arg.Any<CancellationToken>())
            .Returns((new[] { article }.AsEnumerable(), 1));

        var result = await Service.GetArticlesAsync(1, 10);
        var dto = result.Items.Single();

        Assert.Equal(article.Id, dto.Id);
        Assert.Equal("9999999999999", dto.Barcode);
        Assert.Equal(article.Name, dto.Name);
        Assert.Equal(article.Family.ToString(), dto.Family);
        Assert.Equal(article.ColorOrPrint, dto.ColorOrPrint);
        Assert.Equal(new[] { "S", "M" }, dto.AvailableSizes);
    }

    // ── GetArticleByBarcodeAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetArticleByBarcodeAsync_WhenFound_ReturnsMappedDto()
    {
        var article = BuildArticle("3400936123450");
        ArticleRepo.GetByBarcodeAsync("3400936123450", Arg.Any<CancellationToken>())
            .Returns(article);

        var result = await Service.GetArticleByBarcodeAsync("3400936123450");

        Assert.NotNull(result);
        Assert.Equal("3400936123450", result.Barcode);
        Assert.Equal(article.Id, result.Id);
    }

    [Fact]
    public async Task GetArticleByBarcodeAsync_WhenFound_MapsAllFieldsCorrectly()
    {
        // Tue le mutant return null (ligne 91) — vérifie le mapping complet, pas seulement NotNull
        var article = BuildArticle("3400936123450");
        ArticleRepo.GetByBarcodeAsync("3400936123450", Arg.Any<CancellationToken>())
            .Returns(article);

        var result = await Service.GetArticleByBarcodeAsync("3400936123450");

        Assert.NotNull(result);
        Assert.Equal(article.Id, result.Id);
        Assert.Equal("3400936123450", result.Barcode);
        Assert.Equal(article.Name, result.Name);
        Assert.Equal(article.Family.ToString(), result.Family);
        Assert.Equal(article.ColorOrPrint, result.ColorOrPrint);
        Assert.Equal(new[] { "S", "M" }, result.AvailableSizes);
    }

    [Fact]
    public async Task GetArticleByBarcodeAsync_WhenNotFound_ReturnsNull()
    {
        ArticleRepo.GetByBarcodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Article?)null);

        var result = await Service.GetArticleByBarcodeAsync("0000000000000");

        Assert.Null(result);
    }

    // ── GetAllArticlesWithStockAsync ─────────────────────────────────────────

    [Fact]
    public async Task GetAllArticlesWithStockAsync_GroupsEntriesByArticle()
    {
        var articleId = Guid.NewGuid();
        const string storeId = "002";

        var entries = new List<StockEntry>
        {
            new() { Id = Guid.NewGuid(), ArticleId = articleId, StoreId = storeId, Size = Size.S, AvailableQuantity = 2, StoreType = StoreType.Physical, LastUpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), ArticleId = articleId, StoreId = storeId, Size = Size.M, AvailableQuantity = 3, StoreType = StoreType.Physical, LastUpdatedAt = DateTime.UtcNow },
        };

        StockEntryRepo.GetByStoreAsync(storeId, 1, 20, Arg.Any<CancellationToken>())
            .Returns((entries.AsEnumerable(), 2));

        ArticleRepo.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Article> { BuildArticle("1234567890123", articleId) });

        var result = await Service.GetAllArticlesWithStockAsync(storeId, 1, 20);

        var group = result.Items.Single();
        Assert.Equal(articleId, group.ArticleId);
        Assert.Equal(2, group.Stock.Count);
    }

    [Fact]
    public async Task GetAllArticlesWithStockAsync_CapsPageSizeAt50()
    {
        StockEntryRepo.GetByStoreAsync(Arg.Any<string>(), 1, 50, Arg.Any<CancellationToken>())
            .Returns((Enumerable.Empty<StockEntry>(), 0));

        ArticleRepo.GetByIdsAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Article>());

        await Service.GetAllArticlesWithStockAsync("any-store", 1, 999);

        await StockEntryRepo.Received(1).GetByStoreAsync("any-store", 1, 50, Arg.Any<CancellationToken>());
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Article BuildArticle(string barcode, Guid? id = null) => new()
    {
        Id            = id ?? Guid.NewGuid(),
        Barcode       = barcode,
        Name          = "Article Test",
        Family        = ClothingFamily.TSH,
        ColorOrPrint  = "Blanc",
        AvailableSizes = [Size.S, Size.M],
        LastUpdatedAt = DateTime.UtcNow,
    };
}
