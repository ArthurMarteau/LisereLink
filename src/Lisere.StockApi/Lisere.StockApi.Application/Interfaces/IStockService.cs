using Lisere.StockApi.Application.Common;
using Lisere.StockApi.Application.DTOs;

namespace Lisere.StockApi.Application.Interfaces;

public interface IStockService
{
    Task<IEnumerable<StockEntryDto>> GetStockAsync(
        Guid articleId,
        string storeId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<ArticleStockDto>> GetAllArticlesWithStockAsync(
        string storeId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task UpdateStockAsync(UpdateStockDto dto, CancellationToken cancellationToken = default);

    Task<PagedResult<ArticleDto>> GetArticlesAsync(
        int page,
        int pageSize,
        string? query = null,
        CancellationToken cancellationToken = default);

    Task<ArticleDto?> GetArticleByBarcodeAsync(
        string barcode,
        CancellationToken cancellationToken = default);
}
