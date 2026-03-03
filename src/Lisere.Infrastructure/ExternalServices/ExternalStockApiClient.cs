using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;
using Lisere.Application.Common;
using Lisere.Application.DTOs;
using Lisere.Application.Interfaces;
using Lisere.Domain.Entities;
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

    public async Task<PagedResult<ArticleDto>> SearchArticlesAsync(
        string? query,
        string? family,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ForwardJwt();
            var qs = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrEmpty(query)) qs["query"] = query;
            if (!string.IsNullOrEmpty(family)) qs["family"] = family;
            qs["page"] = page.ToString();
            qs["pageSize"] = pageSize.ToString();

            var response = await _httpClient.GetFromJsonAsync<PagedArticlesResponse>(
                $"api/articles?{qs}", cancellationToken);

            if (response is null)
                return new PagedResult<ArticleDto> { Items = [], TotalCount = 0, Page = page, PageSize = pageSize };

            return new PagedResult<ArticleDto>
            {
                Items = response.Items.Select(r => r.MapToArticleDto()),
                TotalCount = response.TotalCount,
                Page = response.Page,
                PageSize = response.PageSize,
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "StockApi indisponible — SearchArticlesAsync a échoué.");
            return new PagedResult<ArticleDto> { Items = [], TotalCount = 0, Page = page, PageSize = pageSize };
        }
    }

    public async Task<ArticleDto?> GetArticleByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        try
        {
            ForwardJwt();
            var response = await _httpClient.GetFromJsonAsync<StockApiArticleResponse>(
                $"api/articles/{barcode}", cancellationToken);

            return response?.MapToArticleDto();
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
