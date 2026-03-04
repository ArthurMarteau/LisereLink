using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Lisere.Tests.Integration;

public class WebhooksControllerTests : IClassFixture<LisereWebApplicationFactory>
{
    private readonly HttpClient _client;

    public WebhooksControllerTests(LisereWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task StockUpdated_WithValidSignature_Returns200()
    {
        var payload = JsonSerializer.Serialize(new { articleId = Guid.NewGuid(), storeId = "paris-opera" });
        var signature = ComputeHmacSha256(LisereWebApplicationFactory.WebhookSecret, payload);

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/stock")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Webhook-Signature", $"sha256={signature}");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task StockUpdated_WithInvalidSignature_Returns401()
    {
        var payload = JsonSerializer.Serialize(new { articleId = Guid.NewGuid(), storeId = "paris-opera" });

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/stock")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Webhook-Signature", "sha256=invalidsignature000000000000000000000000000000000000000000000000");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task StockUpdated_WithMissingSignatureHeader_Returns401()
    {
        var payload = JsonSerializer.Serialize(new { articleId = Guid.NewGuid(), storeId = "paris-opera" });

        var request = new HttpRequestMessage(HttpMethod.Post, "/webhooks/stock")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        // Pas de header X-Webhook-Signature

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static string ComputeHmacSha256(string secret, string payload)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(keyBytes);
        return Convert.ToHexString(hmac.ComputeHash(payloadBytes)).ToLowerInvariant();
    }
}
