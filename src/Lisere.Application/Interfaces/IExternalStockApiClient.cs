using Lisere.Application.Common;
using Lisere.Application.DTOs;

namespace Lisere.Application.Interfaces;

public interface IExternalStockApiClient
{
    Task<IEnumerable<StockDto>> GetStockAsync(Guid articleId, Guid storeId, CancellationToken cancellationToken = default);

    Task<PagedResult<ArticleDto>> SearchArticlesAsync(
        string? query,
        string? family,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ArticleDto?> GetArticleByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
}
