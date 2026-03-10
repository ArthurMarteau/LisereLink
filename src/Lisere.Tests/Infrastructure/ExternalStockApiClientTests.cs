using System.Net;
using Lisere.Infrastructure.ExternalServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Lisere.Tests.Infrastructure;

public class ExternalStockApiClientTests
{
    private static ExternalStockApiClient BuildSut(HttpMessageHandler handler)
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        // HttpContext retourne null → pas de JWT forwarding (comportement nominal sans contexte HTTP)
        accessor.HttpContext.Returns((HttpContext?)null);

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:5200/") };

        return new ExternalStockApiClient(
            httpClient,
            accessor,
            NullLogger<ExternalStockApiClient>.Instance);
    }

    // ── SearchArticlesAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task SearchArticlesAsync_WhenApiResponds_ReturnsMappedPagedResult()
    {
        var articleId = Guid.NewGuid();
        var json = $$"""
            {
                "items": [{
                    "id": "{{articleId}}",
                    "barcode": "1234567890123",
                    "name": "Manteau Test",
                    "family": "COA",
                    "colorOrPrint": "Noir",
                    "availableSizes": ["S", "M"],
                    "price": null,
                    "imageUrl": null
                }],
                "totalCount": 1,
                "page": 1,
                "pageSize": 20
            }
            """;

        var sut = BuildSut(new StaticResponseHandler(HttpStatusCode.OK, json));

        var result = await sut.SearchArticlesAsync("manteau", null, 1, 20);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("1234567890123", result.Items.First().Barcode);
    }

    [Fact]
    public async Task SearchArticlesAsync_WhenApiDown_ReturnsEmptyPagedResult()
    {
        var sut = BuildSut(new ThrowingHandler());

        var result = await sut.SearchArticlesAsync(null, null, 1, 20);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    // ── GetArticleByBarcodeAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetArticleByBarcodeAsync_WhenFound_ReturnsMappedDto()
    {
        var articleId = Guid.NewGuid();
        var json = $$"""
            {
                "id": "{{articleId}}",
                "barcode": "1234567890123",
                "name": "Manteau Test",
                "family": "COA",
                "colorOrPrint": "Noir",
                "availableSizes": ["S"],
                "price": null,
                "imageUrl": null
            }
            """;

        var sut = BuildSut(new StaticResponseHandler(HttpStatusCode.OK, json));

        var result = await sut.GetArticleByBarcodeAsync("1234567890123");

        Assert.NotNull(result);
        Assert.Equal("1234567890123", result.Barcode);
        Assert.Equal(articleId, result.Id);
    }

    [Fact]
    public async Task GetArticleByBarcodeAsync_WhenApiDown_ReturnsNull()
    {
        var sut = BuildSut(new ThrowingHandler());

        var result = await sut.GetArticleByBarcodeAsync("1234567890123");

        Assert.Null(result);
    }

    // ── GetStockAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStockAsync_WhenApiResponds_ReturnsMappedEntries()
    {
        var articleId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var json = $$"""
            [{
                "articleId": "{{articleId}}",
                "storeId": "{{storeId}}",
                "size": "M",
                "availableQuantity": 5
            }]
            """;

        var sut = BuildSut(new StaticResponseHandler(HttpStatusCode.OK, json));

        var result = (await sut.GetStockAsync(articleId, storeId)).ToList();

        Assert.Single(result);
        Assert.Equal("M", result[0].Size);
        Assert.Equal(5, result[0].AvailableQuantity);
    }

    [Fact]
    public async Task GetStockAsync_WhenApiDown_ReturnsEmptyList()
    {
        var sut = BuildSut(new ThrowingHandler());

        var result = await sut.GetStockAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.Empty(result);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private sealed class StaticResponseHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(status)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new HttpRequestException("Erreur réseau simulée.");
    }
}
