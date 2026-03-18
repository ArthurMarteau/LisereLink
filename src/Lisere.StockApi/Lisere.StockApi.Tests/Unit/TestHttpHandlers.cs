using System.Net;

namespace Lisere.StockApi.Tests.Unit;

internal sealed class CapturingHandler(HttpStatusCode statusCode) : HttpMessageHandler
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

internal sealed class ThrowingHandler : HttpMessageHandler
{
    public int CallCount { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CallCount++;
        throw new HttpRequestException("Erreur réseau simulée.");
    }
}
