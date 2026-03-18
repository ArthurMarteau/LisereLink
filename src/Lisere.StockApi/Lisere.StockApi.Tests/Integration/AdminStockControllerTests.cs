using System.Net;
using System.Net.Http.Json;
using Lisere.StockApi.Domain.Enums;
using Lisere.StockApi.Application.DTOs;
using Xunit;

namespace Lisere.StockApi.Tests.Integration;

public class AdminStockControllerTests : StockApiIntegrationTestBase
{
    public AdminStockControllerTests(StockApiWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task UpdateStock_WithoutJwt_Returns401()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/admin/stock", ValidDto());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStock_WithNonAdminJwt_Returns403()
    {
        var client = ClientWithRole("Seller");

        var response = await client.PutAsJsonAsync("/api/admin/stock", ValidDto());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStock_WithAdminJwt_Returns204()
    {
        var client = ClientWithRole("Admin");

        var response = await client.PutAsJsonAsync("/api/admin/stock", ValidDto());

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStock_WithNegativeQuantity_Returns400ProblemDetails()
    {
        var client = ClientWithRole("Admin");

        var dto = new UpdateStockDto
        {
            ArticleId = StockApiWebApplicationFactory.TestArticleId,
            Size = Size.M,
            StoreId = StockApiWebApplicationFactory.TestStoreId,
            NewQuantity = -1
        };

        var response = await client.PutAsJsonAsync("/api/admin/stock", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static UpdateStockDto ValidDto() => new()
    {
        ArticleId = StockApiWebApplicationFactory.TestArticleId,
        Size = Size.M,
        StoreId = StockApiWebApplicationFactory.TestStoreId,
        NewQuantity = 5
    };
}
