using Lisere.StockApi.Domain.Entities;

namespace Lisere.StockApi.Domain.Interfaces;

public interface IStoreRepository
{
    Task<IEnumerable<Store>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Store?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
