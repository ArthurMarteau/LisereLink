using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Lisere.Application.Common;
using Lisere.Application.DTOs;
using Xunit;

namespace Lisere.Tests.Integration;

public class ArticlesControllerTests : IClassFixture<LisereWebApplicationFactory>
{
    private readonly LisereWebApplicationFactory _factory;

    public ArticlesControllerTests(LisereWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ── GET /api/articles ────────────────────────────────────────────────────

    [Fact]
    public async Task Search_WithValidToken_Returns200()
    {
        // StockApi est indisponible en test → ExternalStockApiClient retourne liste vide (comportement attendu)
        var client = await AuthenticatedClientAsync();

        var response = await client.GetAsync("/api/articles?query=manteau&page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ArticleDto>>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Search_WithoutToken_Returns401()
    {
        var response = await _factory.CreateClient().GetAsync("/api/articles");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── GET /api/articles/{barcode} ──────────────────────────────────────────

    [Fact]
    public async Task GetByBarcode_WhenArticleNotFound_Returns404()
    {
        // StockApi est indisponible → GetArticleByBarcodeAsync retourne null → 404
        var client = await AuthenticatedClientAsync();

        var response = await client.GetAsync("/api/articles/9999999999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetByBarcode_WithoutToken_Returns401()
    {
        var response = await _factory.CreateClient().GetAsync("/api/articles/1234567890123");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<HttpClient> AuthenticatedClientAsync()
    {
        var anonClient = _factory.CreateClient();
        var email = $"art_{Guid.NewGuid():N}@test.com";
        const string password = "Test123!";

        await anonClient.PostAsJsonAsync("/api/auth/register",
            new { email, password, firstName = "Test", lastName = "User", role = 0 });

        var loginResp = await anonClient.PostAsJsonAsync("/api/auth/login",
            new { email, password });
        var auth = await loginResp.Content.ReadFromJsonAsync<AuthResponseDto>();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }
}
