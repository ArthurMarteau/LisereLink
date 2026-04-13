using Lisere.StockApi.Application.DTOs;
using Lisere.StockApi.Application.Interfaces;
using Lisere.StockApi.Domain.Interfaces;

namespace Lisere.StockApi.Application.Services;

public class StoreService : IStoreService
{
    private readonly IStoreRepository _storeRepository;

    public StoreService(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }

    public async Task<IEnumerable<StoreDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var stores = await _storeRepository.GetAllAsync(cancellationToken);

        return stores.Select(s => new StoreDto
        {
            Id = s.Id,
            Name = s.Name,
            Code = s.Code,
            Type = s.Type,
        });
    }
}
