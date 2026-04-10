using Lisere.Application.DTOs;

namespace Lisere.Application.Interfaces;

public interface IStoreService
{
    Task<IEnumerable<StoreDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
