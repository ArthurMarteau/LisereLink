using Lisere.Application.Common;
using Lisere.Application.DTOs;
using Lisere.Application.Interfaces;

namespace Lisere.Application.Services;

public class ArticleService : IArticleService
{
    private readonly IExternalStockApiClient _stockApiClient;

    public ArticleService(IExternalStockApiClient stockApiClient)
    {
        _stockApiClient = stockApiClient;
    }

    public async Task<PagedResult<ArticleDto>> SearchAsync(
        string? query,
        string? family,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        return await _stockApiClient.SearchArticlesAsync(query, family, page, pageSize, cancellationToken);
    }

    public Task<ArticleDto?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        return _stockApiClient.GetArticleByBarcodeAsync(barcode, cancellationToken);
    }
}
