using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Lisere.API.Controllers;

[ApiController]
[Route("webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly IConnectionMultiplexer _multiplexer;
    private readonly IConfiguration _configuration;

    public WebhooksController(IConnectionMultiplexer multiplexer, IConfiguration configuration)
    {
        _multiplexer = multiplexer;
        _configuration = configuration;
    }

    [HttpPost("stock")]
    public async Task<IActionResult> StockUpdated()
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();

        if (!VerifySignature(rawBody))
            return Unauthorized();

        var payload = JsonSerializer.Deserialize<StockWebhookPayload>(
            rawBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (payload is null)
            return BadRequest();

        var pattern = $"stock:{payload.ArticleId}:{payload.StoreId}:*";
        var server = _multiplexer.GetServer(_multiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: pattern).ToArray();

        if (keys.Length > 0)
            await _multiplexer.GetDatabase().KeyDeleteAsync(keys);

        return Ok();
    }

    private bool VerifySignature(string rawBody)
    {
        var header = Request.Headers["X-Webhook-Signature"].FirstOrDefault();
        if (string.IsNullOrEmpty(header) || !header.StartsWith("sha256=", StringComparison.Ordinal))
            return false;

        var receivedHex = header["sha256=".Length..];
        var secret = _configuration["Webhooks:Secret"] ?? string.Empty;

        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(rawBody);
        using var hmac = new HMACSHA256(keyBytes);
        var expectedHex = Convert.ToHexString(hmac.ComputeHash(payloadBytes)).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(receivedHex.ToLowerInvariant()),
            Encoding.UTF8.GetBytes(expectedHex));
    }

    private record StockWebhookPayload(Guid ArticleId, string StoreId);
}
