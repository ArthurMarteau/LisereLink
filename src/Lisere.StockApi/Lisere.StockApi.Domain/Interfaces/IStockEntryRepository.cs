using Lisere.StockApi.Domain.Entities;

namespace Lisere.StockApi.Domain.Interfaces;

public interface IStockEntryRepository
{
    Task<IEnumerable<StockEntry>> GetByArticleAndStoreAsync(
        Guid articleId,
        string storeId,
        CancellationToken cancellationToken = default);

    Task<(IEnumerable<StockEntry> Items, int TotalCount)> GetByStoreAsync(
        string storeId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(StockEntry entry, CancellationToken cancellationToken = default);
}
