using Lisere.Application.DTOs;
using Lisere.Application.Interfaces;

namespace Lisere.Application.Services;

public class StoreService : IStoreService
{
    private readonly IExternalStockApiClient _apiClient;

    public StoreService(IExternalStockApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<IEnumerable<StoreDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => _apiClient.GetStoresAsync(cancellationToken);
}
