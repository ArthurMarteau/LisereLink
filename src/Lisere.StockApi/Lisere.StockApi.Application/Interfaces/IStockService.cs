using Lisere.StockApi.Application.DTOs;

namespace Lisere.StockApi.Application.Interfaces;

public interface IStockService
{
    Task<IEnumerable<StockEntryDto>> GetStockByArticleAsync(
        Guid articleId,
        string storeId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<ArticleStockDto>> GetStockByStoreAsync(
        string storeId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task UpdateStockAsync(UpdateStockDto dto, CancellationToken cancellationToken = default);
}
