using System.Net;
using System.Net.Http.Json;
using Lisere.StockApi.Application.Common;
using Lisere.StockApi.Application.DTOs;
using Xunit;

namespace Lisere.StockApi.Tests.Integration;

public class ArticlesControllerTests : IClassFixture<StockApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ArticlesControllerTests(StockApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithPagedResult()
    {
        var response = await _client.GetAsync("/api/articles?page=1&pageSize=50");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ArticleDto>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 1);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GetByBarcode_WhenArticleExists_ReturnsOkWithArticleDto()
    {
        var response = await _client.GetAsync($"/api/articles/{StockApiWebApplicationFactory.TestBarcode}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var article = await response.Content.ReadFromJsonAsync<ArticleDto>();
        Assert.NotNull(article);
        Assert.Equal(StockApiWebApplicationFactory.TestBarcode, article.Barcode);
        Assert.Equal(StockApiWebApplicationFactory.TestArticleId, article.Id);
    }

    [Fact]
    public async Task GetByBarcode_WhenArticleDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/articles/0000000000000");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
