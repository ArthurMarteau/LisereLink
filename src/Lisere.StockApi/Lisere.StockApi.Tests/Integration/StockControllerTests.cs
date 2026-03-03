using System.Net;
using System.Net.Http.Json;
using Lisere.StockApi.Application.DTOs;
using Xunit;

namespace Lisere.StockApi.Tests.Integration;

public class StockControllerTests : IClassFixture<StockApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public StockControllerTests(StockApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetByArticle_WhenStockExists_ReturnsOkWithEntriesBySize()
    {
        var url = $"/api/stock/{StockApiWebApplicationFactory.TestArticleId}" +
                  $"?storeId={StockApiWebApplicationFactory.TestStoreId}";

        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var entries = await response.Content.ReadFromJsonAsync<List<StockEntryDto>>();
        Assert.NotNull(entries);
        Assert.NotEmpty(entries);
        Assert.All(entries, e => Assert.Equal(StockApiWebApplicationFactory.TestStoreId, e.StoreId));
    }
}
