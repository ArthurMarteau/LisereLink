using System.Net;
using System.Security.Cryptography;
using System.Text;
using Lisere.StockApi.Infrastructure.Webhooks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Lisere.StockApi.Tests.Unit;

public class WebhookNotifierTests
{
    private const string Secret = "test-webhook-secret";
    private const string Url = "https://test.example.com/webhooks/stock";

    // Backoff nul pour que les tests ne durent pas 7 secondes
    private static readonly TimeSpan[] NoDelay = [TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero];

    private static WebhookNotifier BuildSut(HttpMessageHandler handler) =>
        BuildSutWithLogger(handler, NullLogger<WebhookNotifier>.Instance);

    private static WebhookNotifier BuildSutWithLogger(HttpMessageHandler handler, ILogger<WebhookNotifier> logger) =>
        new(
            new HttpClient(handler) { BaseAddress = new Uri(Url) },
            Options.Create(new WebhookOptions { LisereApiUrl = Url, Secret = Secret }),
            logger,
            NoDelay);

    // ── Signature correcte ────────────────────────────────────────────────────

    [Fact]
    public async Task NotifyStockUpdatedAsync_ValidConfig_SendsRequestWithCorrectSignature()
    {
        var handler = new CapturingHandler(HttpStatusCode.OK);
        var sut = BuildSut(handler);
        var articleId = Guid.NewGuid();

        await sut.NotifyStockUpdatedAsync(articleId, "002");

        Assert.Single(handler.Captures);
        var (body, sigHeader) = handler.Captures[0];

        Assert.NotNull(sigHeader);
        Assert.StartsWith("sha256=", sigHeader);
        Assert.True(sigHeader.Length > 7); // "sha256=" + au moins 1 char — tue la mutation string.Empty

        var expected = $"sha256={ComputeHmacSha256(Secret, body)}";
        Assert.Equal(expected, sigHeader);
    }

    // ── Retry 4 tentatives sur 500 ────────────────────────────────────────────

    [Fact]
    public async Task NotifyStockUpdatedAsync_HttpFailure_RetriesUpTo4Times()
    {
        var handler = new CapturingHandler(HttpStatusCode.InternalServerError);
        var sut = BuildSut(handler);

        await sut.NotifyStockUpdatedAsync(Guid.NewGuid(), "002");

        Assert.Equal(4, handler.Captures.Count);
    }

    // ── Exception réseau — ne propage pas ─────────────────────────────────────

    [Fact]
    public async Task NotifyStockUpdatedAsync_NetworkException_DoesNotThrow()
    {
        var handler = new ThrowingHandler();
        var sut = BuildSut(handler);

        var exception = await Record.ExceptionAsync(
            () => sut.NotifyStockUpdatedAsync(Guid.NewGuid(), "002"));

        Assert.Null(exception);
        Assert.Equal(4, handler.CallCount);
    }

    // ── Retry : succès immédiat ───────────────────────────────────────────

    [Fact]
    public async Task NotifyStockUpdatedAsync_SuccessOnFirstAttempt_DoesNotRetry()
    {
        var handler = new CapturingHandler(HttpStatusCode.OK);
        var sut = BuildSut(handler);

        await sut.NotifyStockUpdatedAsync(Guid.NewGuid(), "002");

        Assert.Single(handler.Captures);
    }

    // ── Retry : 2 échecs puis succès ──────────────────────────────────────

    [Fact]
    public async Task NotifyStockUpdatedAsync_FailsTwiceThenSucceeds_Retries()
    {
        var handler = new SequentialResponseHandler(
            HttpStatusCode.InternalServerError,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.OK);

        var sut = new WebhookNotifier(
            new HttpClient(handler) { BaseAddress = new Uri(Url) },
            Options.Create(new WebhookOptions { LisereApiUrl = Url, Secret = Secret }),
            NullLogger<WebhookNotifier>.Instance,
            NoDelay);

        await sut.NotifyStockUpdatedAsync(Guid.NewGuid(), "002");

        Assert.Equal(3, handler.CallCount);
    }

    // ── Retry : non-2xx non considéré comme succès ────────────────────────

    [Fact]
    public async Task NotifyStockUpdatedAsync_NonSuccessStatus_IsNotConsideredSuccess()
    {
        var handler = new CapturingHandler(HttpStatusCode.NotFound);
        var sut = BuildSut(handler);

        await sut.NotifyStockUpdatedAsync(Guid.NewGuid(), "002");

        Assert.Equal(4, handler.Captures.Count);
    }

    // ── Retry : exactement 4 tentatives puis abandon ──────────────────────

    [Fact]
    public async Task NotifyStockUpdatedAsync_ExactlyFourAttempts_ThenGivesUp()
    {
        var handler = new CapturingHandler(HttpStatusCode.InternalServerError);
        var sut = BuildSut(handler);

        await sut.NotifyStockUpdatedAsync(Guid.NewGuid(), "002");

        Assert.Equal(4, handler.Captures.Count);
    }

    // ── Logger : numéro de tentative ─────────────────────────────────────

    [Fact]
    public async Task NotifyStockUpdatedAsync_OnNonSuccessResponse_LogsAttemptNumber()
    {
        // Tue les mutants attempt + 1 → attempt ou attempt - 1 (lignes 51/55 de WebhookNotifier)
        var logger = Substitute.For<ILogger<WebhookNotifier>>();
        var handler = new CapturingHandler(HttpStatusCode.InternalServerError);
        var sut = BuildSutWithLogger(handler, logger);

        await sut.NotifyStockUpdatedAsync(Guid.NewGuid(), "002");

        var logCalls = logger.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == "Log")
            .ToList();
        Assert.Contains(logCalls, c =>
        {
            var args = c.GetArguments();
            return args[0] is LogLevel.Warning
                && args[2]?.ToString()?.Contains("1/4") == true;
        });
    }

    [Fact]
    public async Task NotifyStockUpdatedAsync_OnNetworkException_LogsWarning()
    {
        // Tue le mutant LogWarning supprimé dans la branche catch (ligne 62-63)
        var logger = Substitute.For<ILogger<WebhookNotifier>>();
        var handler = new ThrowingHandler();
        var sut = BuildSutWithLogger(handler, logger);

        await sut.NotifyStockUpdatedAsync(Guid.NewGuid(), "002");

        var logCalls = logger.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == "Log")
            .ToList();
        Assert.Contains(logCalls, c => c.GetArguments()[0] is LogLevel.Warning);
    }

    // ── Retry : 201 Created considéré comme succès ────────────────────────

    [Fact]
    public async Task NotifyStockUpdatedAsync_201CreatedIsConsideredSuccess_NoRetry()
    {
        // Tue le mutant IsSuccessStatusCode → !IsSuccessStatusCode (ligne 58)
        var handler = new CapturingHandler(HttpStatusCode.Created);
        var sut = BuildSut(handler);

        await sut.NotifyStockUpdatedAsync(Guid.NewGuid(), "002");

        Assert.Single(handler.Captures);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string ComputeHmacSha256(string secret, string payload)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(keyBytes);
        return Convert.ToHexString(hmac.ComputeHash(payloadBytes)).ToLowerInvariant();
    }

    private sealed class SequentialResponseHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode[] _statusCodes;
        private int _callIndex;

        public SequentialResponseHandler(params HttpStatusCode[] statusCodes)
            => _statusCodes = statusCodes;

        public int CallCount => _callIndex;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var code = _callIndex < _statusCodes.Length
                ? _statusCodes[_callIndex]
                : _statusCodes[^1];
            _callIndex++;
            return Task.FromResult(new HttpResponseMessage(code));
        }
    }
}
