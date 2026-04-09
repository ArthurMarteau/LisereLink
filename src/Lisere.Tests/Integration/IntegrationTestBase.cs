using System.Net.Http.Headers;
using System.Net.Http.Json;
using Lisere.Application.DTOs;
using Xunit;

namespace Lisere.Tests.Integration;

public abstract class IntegrationTestBase : IClassFixture<LisereWebApplicationFactory>
{
    protected readonly LisereWebApplicationFactory Factory;

    protected IntegrationTestBase(LisereWebApplicationFactory factory)
        => Factory = factory;

    protected async Task<HttpClient> AuthenticatedClientAsync(
        LisereWebApplicationFactory? factory = null)
    {
        var f = factory ?? Factory;
        var anonClient = f.CreateClient();
        var email = $"test_{Guid.NewGuid():N}@test.com";
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

    protected async Task RegisterUserAsync(LisereWebApplicationFactory f, string email, string password)
    {
        var dto = new { email, password, firstName = "Test", lastName = "User", role = 0 };
        (await f.CreateClient().PostAsJsonAsync("/api/auth/register", dto))
            .EnsureSuccessStatusCode();
    }
}
