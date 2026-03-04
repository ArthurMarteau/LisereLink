using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Lisere.Application.Common;
using Lisere.Application.DTOs;
using Xunit;

namespace Lisere.Tests.Integration;

public class AuthControllerTests : IClassFixture<LisereWebApplicationFactory>
{
    private readonly LisereWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(LisereWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // ── POST /api/auth/register ───────────────────────────────────────────────

    [Fact]
    public async Task Register_WithValidData_Returns201WithGuid()
    {
        var dto = new
        {
            email = $"register_{Guid.NewGuid():N}@test.com",
            password = "Test123!",
            firstName = "Jean",
            lastName = "Dupont",
            role = 0 // Seller
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id = Guid.Parse(body.GetProperty("id").GetString()!);
        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task Register_WithExistingEmail_Returns400()
    {
        var email = $"dup_{Guid.NewGuid():N}@test.com";
        var dto = new { email, password = "Test123!", firstName = "A", lastName = "B", role = 0 };

        var first = await _client.PostAsJsonAsync("/api/auth/register", dto);
        first.EnsureSuccessStatusCode();

        var second = await _client.PostAsJsonAsync("/api/auth/register", dto);

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    // ── POST /api/auth/login ──────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithNonEmptyTokenAndFutureExpiry()
    {
        var email = $"login_{Guid.NewGuid():N}@test.com";
        const string password = "Test123!";
        await RegisterUserAsync(email, password);

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrEmpty(auth.Token));
        Assert.True(auth.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var email = $"wp_{Guid.NewGuid():N}@test.com";
        await RegisterUserAsync(email, "Test123!");

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "WrongPassword!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = $"ghost_{Guid.NewGuid():N}@test.com", password = "Test123!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Accès protégé ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRequests_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/requests");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetRequests_WithValidToken_Returns200WithEmptyPagedResult()
    {
        var email = $"auth_{Guid.NewGuid():N}@test.com";
        const string password = "Test123!";
        await RegisterUserAsync(email, password);

        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });
        loginResp.EnsureSuccessStatusCode();
        var auth = await loginResp.Content.ReadFromJsonAsync<AuthResponseDto>();

        // Nouveau client isolé pour ne pas polluer les headers partagés
        var authClient = _factory.CreateClient();
        authClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.Token);

        var response = await authClient.GetAsync("/api/requests");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<RequestDto>>();
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task RegisterUserAsync(string email, string password)
    {
        var dto = new { email, password, firstName = "Test", lastName = "User", role = 0 };
        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
        response.EnsureSuccessStatusCode();
    }
}
