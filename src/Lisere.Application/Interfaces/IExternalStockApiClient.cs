using Lisere.Application.Common;
using Lisere.Application.DTOs;

namespace Lisere.Application.Interfaces;

public interface IExternalStockApiClient
{
    Task<IEnumerable<StoreDto>> GetStoresAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<StockDto>> GetStockAsync(Guid articleId, string storeId, CancellationToken cancellationToken = default);

    Task<PagedResult<ArticleDto>> SearchArticlesAsync(
        string? query,
        string? family,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ArticleDto?> GetArticleByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);

    Task<ArticleDto?> GetArticleByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
