using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Lisere.Application.DTOs;
using Lisere.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Lisere.Tests.Integration;

public class RequestsControllerTests : IClassFixture<LisereWebApplicationFactory>
{
    private readonly LisereWebApplicationFactory _factory;

    public RequestsControllerTests(LisereWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ── GET /api/requests/{id} ───────────────────────────────────────────────

    [Fact]
    public async Task GetById_WithNonExistentId_Returns404()
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.GetAsync($"/api/requests/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/requests/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── DELETE /api/requests/{id} ────────────────────────────────────────────

    [Fact]
    public async Task Cancel_WithNonExistentId_Returns404WithProblemDetails()
    {
        var client = await AuthenticatedClientAsync();

        var response = await client.DeleteAsync($"/api/requests/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    // ── POST /api/requests ───────────────────────────────────────────────────

    [Fact]
    public async Task Create_WithEmptyLines_Returns400()
    {
        var client = await AuthenticatedClientAsync();
        var dto = new CreateRequestDto
        {
            SellerId = Guid.NewGuid(),
            // Lines is empty — [MinLength(1)] should trigger model validation
        };

        var response = await client.PostAsJsonAsync("/api/requests", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WhenStockUnavailable_Returns400WithProblemDetails()
    {
        // En environnement de test, StockApi est indisponible → IsAvailableAsync retourne false
        // → BusinessException → 400 ProblemDetails
        var client = await AuthenticatedClientAsync();
        var dto = new CreateRequestDto
        {
            SellerId = Guid.NewGuid(),
            Lines =
            [
                new CreateRequestLineDto
                {
                    ArticleId           = Guid.NewGuid(),
                    ArticleName         = "Manteau",
                    ArticleColorOrPrint = "Noir",
                    ArticleBarcode      = "1234567890123",
                    RequestedSizes      = ["M"],
                    Quantity            = 1,
                }
            ]
        };

        var response = await client.PostAsJsonAsync("/api/requests", dto);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Create_WithAvailableStock_Returns201()
    {
        // Factory spécialisée qui mocke IStockService → stock toujours disponible
        var factory = new LisereWebApplicationFactory();
        // On surcharge IStockService via factory inline
        using var customFactory = new StockAvailableFactory();
        var client = await AuthenticatedClientAsync(customFactory);

        var dto = new CreateRequestDto
        {
            SellerId = Guid.NewGuid(),
            Lines =
            [
                new CreateRequestLineDto
                {
                    ArticleId           = Guid.NewGuid(),
                    ArticleName         = "Manteau",
                    ArticleColorOrPrint = "Noir",
                    ArticleBarcode      = "1234567890123",
                    RequestedSizes      = ["M"],
                    Quantity            = 1,
                }
            ]
        };

        var response = await client.PostAsJsonAsync("/api/requests", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<RequestDto>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
    }

    // ── PUT /api/requests/{id} ───────────────────────────────────────────────

    [Fact]
    public async Task Update_WithNonExistentId_Returns404()
    {
        var client = await AuthenticatedClientAsync();
        var dto = new UpdateRequestDto { Status = Domain.Enums.RequestStatus.InProgress };

        var response = await client.PutAsJsonAsync($"/api/requests/{Guid.NewGuid()}", dto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<HttpClient> AuthenticatedClientAsync(
        LisereWebApplicationFactory? factory = null)
    {
        var f = factory ?? _factory;
        var anonClient = f.CreateClient();

        var email = $"req_{Guid.NewGuid():N}@test.com";
        const string password = "Test123!";

        await anonClient.PostAsJsonAsync("/api/auth/register",
            new { email, password, firstName = "Test", lastName = "User", role = 0 });

        var loginResp = await anonClient.PostAsJsonAsync("/api/auth/login",
            new { email, password });
        var auth = await loginResp.Content.ReadFromJsonAsync<AuthResponseDto>();

        var client = f.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }
}

/// <summary>
/// Factory avec IStockService mocké pour simuler du stock disponible.
/// </summary>
file sealed class StockAvailableFactory : LisereWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            // Retire le vrai StockService et injecte un mock qui retourne toujours disponible
            var descriptor = services.FirstOrDefault(
                d => d.ServiceType == typeof(IStockService));
            if (descriptor != null) services.Remove(descriptor);

            var mock = Substitute.For<IStockService>();
            mock.IsAvailableAsync(
                    Arg.Any<Guid>(),
                    Arg.Any<string>(),
                    Arg.Any<CancellationToken>())
                .Returns(true);
            mock.GetAvailabilityAsync(
                    Arg.Any<Guid>(),
                    Arg.Any<string>(),
                    Arg.Any<CancellationToken>())
                .Returns(5);

            services.AddScoped<IStockService>(_ => mock);
        });
    }
}
