using Lisere.Application.Common;
using Xunit;
using Lisere.Application.DTOs;
using Lisere.Application.Interfaces;
using Lisere.Application.Services;
using NSubstitute;

namespace Lisere.Tests.Application.Services;

public class ArticleServiceTests
{
    private readonly IExternalStockApiClient _stockApiClient;
    private readonly ArticleService _sut;

    public ArticleServiceTests()
    {
        _stockApiClient = Substitute.For<IExternalStockApiClient>();
        _sut = new ArticleService(_stockApiClient);
    }

    // -------------------------------------------------------------------------
    // SearchAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SearchAsync_DelegatesToStockApiClientAndReturnsPagedResult()
    {
        var expected = new PagedResult<ArticleDto>
        {
            Items = [BuildArticleDto(), BuildArticleDto()],
            TotalCount = 2,
            Page = 1,
            PageSize = 20,
        };
        _stockApiClient
            .SearchArticlesAsync("manteau", "COA", 1, 20, Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _sut.SearchAsync("manteau", "COA", 1, 20);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
        await _stockApiClient.Received(1)
            .SearchArticlesAsync("manteau", "COA", 1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchAsync_CapsPageSizeAt50()
    {
        var expected = new PagedResult<ArticleDto>
        {
            Items = [],
            TotalCount = 0,
            Page = 1,
            PageSize = 50,
        };
        _stockApiClient
            .SearchArticlesAsync(null, null, 1, 50, Arg.Any<CancellationToken>())
            .Returns(expected);

        await _sut.SearchAsync(null, null, 1, 999);

        await _stockApiClient.Received(1)
            .SearchArticlesAsync(null, null, 1, 50, Arg.Any<CancellationToken>());
    }

    // -------------------------------------------------------------------------
    // GetByBarcodeAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByBarcodeAsync_WithExistingArticle_ReturnsDto()
    {
        var barcode = "1234567890123";
        var article = BuildArticleDto(barcode);
        _stockApiClient.GetArticleByBarcodeAsync(barcode, Arg.Any<CancellationToken>())
            .Returns(article);

        var result = await _sut.GetByBarcodeAsync(barcode);

        Assert.NotNull(result);
        Assert.Equal(barcode, result.Barcode);
    }

    [Fact]
    public async Task GetByBarcodeAsync_WithNonExistingArticle_ReturnsNull()
    {
        var barcode = "0000000000000";
        _stockApiClient.GetArticleByBarcodeAsync(barcode, Arg.Any<CancellationToken>())
            .Returns((ArticleDto?)null);

        var result = await _sut.GetByBarcodeAsync(barcode);

        Assert.Null(result);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static ArticleDto BuildArticleDto(string barcode = "1234567890123") => new()
    {
        Id           = Guid.NewGuid(),
        Barcode      = barcode,
        Name         = "Manteau Test",
        Family       = "COA",
        ColorOrPrint = "Noir",
        AvailableSizes = ["S", "M", "L"],
    };
}
