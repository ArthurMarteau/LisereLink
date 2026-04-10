using Lisere.Application.DTOs;

namespace Lisere.Application.Interfaces;

public interface IStockService
{
    Task<int> GetAvailabilityAsync(Guid articleId, string size, CancellationToken cancellationToken = default);

    Task<bool> IsAvailableAsync(Guid articleId, string size, CancellationToken cancellationToken = default);

    Task<IEnumerable<StockDto>> GetStockForStoreAsync(Guid articleId, string storeId, CancellationToken cancellationToken = default);
}
