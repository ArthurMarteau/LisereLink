using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Lisere.StockApi.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lisere.StockApi.Infrastructure.Webhooks;

public class WebhookNotifier : IWebhookNotifier
{
    private static readonly TimeSpan[] DefaultBackoff = [TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4)];

    private readonly HttpClient _httpClient;
    private readonly WebhookOptions _options;
    private readonly ILogger<WebhookNotifier> _logger;
    private readonly TimeSpan[] _backoff;

    public WebhookNotifier(HttpClient httpClient, IOptions<WebhookOptions> options, ILogger<WebhookNotifier> logger)
        : this(httpClient, options, logger, DefaultBackoff) { }

    internal WebhookNotifier(HttpClient httpClient, IOptions<WebhookOptions> options, ILogger<WebhookNotifier> logger, TimeSpan[] backoff)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _backoff = backoff;
    }

    public async Task NotifyStockUpdatedAsync(Guid articleId, string storeId)
    {
        var payload = JsonSerializer.Serialize(new { articleId, storeId });
        var signature = ComputeHmacSha256(_options.Secret, payload);

        for (var attempt = 0; attempt < 4; attempt++)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, _options.LisereApiUrl)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };
                request.Headers.Add("X-Webhook-Signature", $"sha256={signature}");

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                    return;

                _logger.LogWarning(
                    "Webhook réponse non-2xx (tentative {Attempt}/4) : {StatusCode}.",
                    attempt + 1, (int)response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Webhook échec réseau (tentative {Attempt}/4).", attempt + 1);
            }

            if (attempt < _backoff.Length)
                await Task.Delay(_backoff[attempt]);
        }

        _logger.LogWarning(
            "Webhook définitivement échoué après 4 tentatives (articleId={ArticleId}, storeId={StoreId}).",
            articleId, storeId);
    }

    private static string ComputeHmacSha256(string secret, string payload)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(keyBytes);
        return Convert.ToHexString(hmac.ComputeHash(payloadBytes)).ToLowerInvariant();
    }
}
