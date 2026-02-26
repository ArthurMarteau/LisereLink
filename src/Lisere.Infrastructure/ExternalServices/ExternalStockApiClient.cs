using System.Net.Http.Headers;
using System.Net.Http.Json;
using Lisere.Domain.Entities;
using Lisere.Domain.Enums;
using Lisere.Domain.Interfaces;
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
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<StockApiArticleResponse>>(
                "api/articles", cancellationToken) ?? [];

            return response.Select(MapToArticle);
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

            return response is null ? null : MapToArticle(response);
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
                $"api/stock?articleId={articleId}&storeId={storeId}", cancellationToken) ?? [];

            return response.Select(s => new Stock
            {
                ArticleId = s.ArticleId,
                Size = Enum.Parse<Size>(s.Size),
                AvailableQuantity = s.AvailableQuantity,
            });
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

    private static Article MapToArticle(StockApiArticleResponse r) => new()
    {
        Id = r.Id,
        Barcode = r.Barcode,
        Name = r.Name,
        Family = Enum.Parse<ClothingFamily>(r.Family),
        ColorOrPrint = r.ColorOrPrint,
        AvailableSizes = r.AvailableSizes
            .Select(s => Enum.Parse<Size>(s))
            .ToList(),
        Price = r.Price,
        ImageUrl = r.ImageUrl,
    };

    // Internal response shapes matching StockApi JSON
    private sealed record StockApiArticleResponse(
        Guid Id,
        string Barcode,
        string Name,
        string Family,
        string ColorOrPrint,
        List<string> AvailableSizes,
        decimal? Price,
        string? ImageUrl);

    private sealed record StockApiStockEntryResponse(
        Guid ArticleId,
        Guid StoreId,
        string Size,
        int AvailableQuantity);
}
