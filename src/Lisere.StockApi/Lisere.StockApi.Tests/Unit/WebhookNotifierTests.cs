using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Lisere.StockApi.Infrastructure.Webhooks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Lisere.StockApi.Tests.Unit;

public class WebhookNotifierTests
{
    private const string Secret = "test-webhook-secret";
    private const string Url = "https://test.example.com/webhooks/stock";

    // Backoff nul pour que les tests ne durent pas 7 secondes
    private static readonly TimeSpan[] NoDelay = [TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero];

    private static WebhookNotifier BuildSut(HttpMessageHandler handler) =>
        new(
            new HttpClient(handler) { BaseAddress = new Uri(Url) },
            Options.Create(new WebhookOptions { LisereApiUrl = Url, Secret = Secret }),
            NullLogger<WebhookNotifier>.Instance,
            NoDelay);

    // ── Signature correcte ────────────────────────────────────────────────────

    [Fact]
    public async Task NotifyStockUpdatedAsync_ValidConfig_SendsRequestWithCorrectSignature()
    {
        var handler = new CapturingHandler(HttpStatusCode.OK);
        var sut = BuildSut(handler);
        var articleId = Guid.NewGuid();

        await sut.NotifyStockUpdatedAsync(articleId, "paris-opera");

        Assert.Single(handler.Captures);
        var (body, sigHeader) = handler.Captures[0];

        Assert.NotNull(sigHeader);
        Assert.StartsWith("sha256=", sigHeader);

        var expected = $"sha256={ComputeHmacSha256(Secret, body)}";
        Assert.Equal(expected, sigHeader);
    }

    // ── Retry 4 tentatives sur 500 ────────────────────────────────────────────

    [Fact]
    public async Task NotifyStockUpdatedAsync_HttpFailure_RetriesUpTo4Times()
    {
        var handler = new CapturingHandler(HttpStatusCode.InternalServerError);
        var sut = BuildSut(handler);

        await sut.NotifyStockUpdatedAsync(Guid.NewGuid(), "paris-opera");

        Assert.Equal(4, handler.Captures.Count);
    }

    // ── Exception réseau — ne propage pas ─────────────────────────────────────

    [Fact]
    public async Task NotifyStockUpdatedAsync_NetworkException_DoesNotThrow()
    {
        var handler = new ThrowingHandler();
        var sut = BuildSut(handler);

        var exception = await Record.ExceptionAsync(
            () => sut.NotifyStockUpdatedAsync(Guid.NewGuid(), "paris-opera"));

        Assert.Null(exception);
        Assert.Equal(4, handler.CallCount);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string ComputeHmacSha256(string secret, string payload)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(keyBytes);
        return Convert.ToHexString(hmac.ComputeHash(payloadBytes)).ToLowerInvariant();
    }

    private sealed class CapturingHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public List<(string Body, string? SignatureHeader)> Captures { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content != null
                ? await request.Content.ReadAsStringAsync(cancellationToken)
                : string.Empty;

            var sig = request.Headers.TryGetValues("X-Webhook-Signature", out var vals)
                ? vals.FirstOrDefault()
                : null;

            Captures.Add((body, sig));
            return new HttpResponseMessage(statusCode);
        }
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            throw new HttpRequestException("Erreur réseau simulée.");
        }
    }
}
