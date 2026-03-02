using System.Net.Http.Headers;
using System.Net.Http.Json;
using Lisere.Domain.Entities;
using Lisere.Domain.Interfaces;
using Lisere.Infrastructure.ExternalServices.Dtos;
using Lisere.Infrastructure.ExternalServices.Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Lisere.Infrastructure.ExternalServices;

public class ExternalStockApiClient : IExternalStockApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ExternalStockApiClient> _logger;

    public ExternalStockApiClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ExternalStockApiClient> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<IEnumerable<Article>> GetArticlesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            ForwardJwt();
            var response = await _httpClient.GetFromJsonAsync<PagedArticlesResponse>(
                "api/articles", cancellationToken);

            return response?.Items.Select(r => r.MapToArticle()) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "StockApi indisponible — GetArticlesAsync a échoué.");
            return [];
        }
    }

    public async Task<Article?> GetArticleByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        try
        {
            ForwardJwt();
            var response = await _httpClient.GetFromJsonAsync<StockApiArticleResponse>(
                $"api/articles/{barcode}", cancellationToken);

            return response?.MapToArticle();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "StockApi indisponible — GetArticleByBarcodeAsync({Barcode}) a échoué.", barcode);
            return null;
        }
    }

    public async Task<IEnumerable<Stock>> GetStockAsync(
        Guid articleId,
        Guid storeId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ForwardJwt();
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<StockApiStockEntryResponse>>(
                $"api/stock/{articleId}?storeId={storeId}", cancellationToken) ?? [];

            return response.Select(s => s.MapToStock());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "StockApi indisponible — GetStockAsync({ArticleId}, {StoreId}) a échoué.",
                articleId, storeId);
            return [];
        }
    }

    private void ForwardJwt()
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(authHeader))
            _httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);
    }
}
