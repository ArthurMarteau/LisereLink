using Lisere.Domain.Enums;
using Lisere.StockApi.Domain.Entities;

namespace Lisere.StockApi.Domain.Interfaces;

public interface IStockEntryRepository
{
    Task<IEnumerable<StockEntry>> GetByArticleAsync(
        Guid articleId,
        string storeId,
        CancellationToken cancellationToken = default);

    Task<StockEntry?> GetByArticleAndSizeAsync(
        Guid articleId,
        Size size,
        string storeId,
        CancellationToken cancellationToken = default);

    Task<(IEnumerable<StockEntry> Items, int TotalCount)> GetByStoreAsync(
        string storeId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(StockEntry entry, CancellationToken cancellationToken = default);

    Task UpsertRangeAsync(IEnumerable<StockEntry> entries, CancellationToken cancellationToken = default);
}
