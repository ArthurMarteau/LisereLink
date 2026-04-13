using Lisere.StockApi.Application.DTOs;

namespace Lisere.StockApi.Application.Interfaces;

public interface IStoreService
{
    Task<IEnumerable<StoreDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
