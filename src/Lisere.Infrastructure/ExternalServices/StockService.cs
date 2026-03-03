using System.Text;
using System.Text.Json;
using Lisere.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Lisere.Infrastructure.ExternalServices;

public class StockService : IStockService
{
    private readonly IExternalStockApiClient _apiClient;
    private readonly IDistributedCache _cache;
    private readonly ILogger<StockService> _logger;
    private readonly Guid _storeId;

    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
    };

    public StockService(
        IExternalStockApiClient apiClient,
        IDistributedCache cache,
        IConfiguration configuration,
        ILogger<StockService> logger)
    {
        _apiClient = apiClient;
        _cache = cache;
        _logger = logger;
        _storeId = Guid.TryParse(configuration["Store:StoreId"], out var id) ? id : Guid.Empty;
    }

    public async Task<int> GetAvailabilityAsync(
        Guid articleId,
        string size,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"stock:{articleId}:{_storeId}:{size}";

        var cached = await _cache.GetAsync(cacheKey, cancellationToken);
        if (cached is not null)
            return JsonSerializer.Deserialize<int>(Encoding.UTF8.GetString(cached));

        var stocks = await _apiClient.GetStockAsync(articleId, _storeId, cancellationToken);
        var entry = stocks.FirstOrDefault(s => s.Size == size);
        var quantity = entry?.AvailableQuantity ?? 0;

        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(quantity));
        await _cache.SetAsync(cacheKey, bytes, CacheOptions, cancellationToken);

        return quantity;
    }

    public async Task<bool> IsAvailableAsync(
        Guid articleId,
        string size,
        CancellationToken cancellationToken = default)
    {
        var quantity = await GetAvailabilityAsync(articleId, size, cancellationToken);
        return quantity > 0;
    }
}
