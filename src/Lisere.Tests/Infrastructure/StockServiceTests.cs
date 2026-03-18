using System.Text;
using System.Text.Json;
using Lisere.Application.DTOs;
using Lisere.Application.Interfaces;
using Lisere.Infrastructure.ExternalServices;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Lisere.Tests.Infrastructure;

public class StockServiceTests
{
    private readonly IExternalStockApiClient _apiClient;
    private readonly IDistributedCache _cache;
    private readonly Guid _storeId = Guid.NewGuid();
    private readonly StockService _sut;

    public StockServiceTests()
    {
        _apiClient = Substitute.For<IExternalStockApiClient>();
        _cache = Substitute.For<IDistributedCache>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Store:StoreId"] = _storeId.ToString()
            })
            .Build();

        _sut = new StockService(_apiClient, _cache, config, NullLogger<StockService>.Instance);
    }

    // ── GetAvailabilityAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetAvailabilityAsync_CacheHit_ReturnsCachedValueWithoutCallingApi()
    {
        var articleId = Guid.NewGuid();
        var cachedBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(7));

        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(cachedBytes);

        var result = await _sut.GetAvailabilityAsync(articleId, "M");

        Assert.Equal(7, result);
        await _apiClient.DidNotReceive()
            .GetStockAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAvailabilityAsync_CacheMiss_CallsApiAndCachesResult()
    {
        var articleId = Guid.NewGuid();

        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        _apiClient.GetStockAsync(articleId, _storeId, Arg.Any<CancellationToken>())
            .Returns(new List<StockDto>
            {
                new() { ArticleId = articleId, Size = "M", AvailableQuantity = 3 }
            });

        var result = await _sut.GetAvailabilityAsync(articleId, "M");

        Assert.Equal(3, result);
        await _cache.Received(1).SetAsync(
            Arg.Is<string>(k => k.StartsWith("stock:")),
            Arg.Any<byte[]>(),
            Arg.Is<DistributedCacheEntryOptions>(o =>
                o.AbsoluteExpirationRelativeToNow == TimeSpan.FromSeconds(30)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAvailabilityAsync_CacheMiss_SizeNotFound_ReturnsZero()
    {
        var articleId = Guid.NewGuid();

        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((byte[]?)null);

        _apiClient.GetStockAsync(articleId, _storeId, Arg.Any<CancellationToken>())
            .Returns(new List<StockDto>
            {
                new() { ArticleId = articleId, Size = "L", AvailableQuantity = 5 }
            });

        var result = await _sut.GetAvailabilityAsync(articleId, "M");

        Assert.Equal(0, result);
    }

    // ── IsAvailableAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_QuantityGreaterThanZero_ReturnsTrue()
    {
        var articleId = Guid.NewGuid();
        var cachedBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(1));

        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(cachedBytes);

        var result = await _sut.IsAvailableAsync(articleId, "M");

        Assert.True(result);
    }

    [Fact]
    public async Task IsAvailableAsync_QuantityIsZero_ReturnsFalse()
    {
        var articleId = Guid.NewGuid();
        var cachedBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(0));

        _cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(cachedBytes);

        var result = await _sut.IsAvailableAsync(articleId, "M");

        Assert.False(result);
    }
}
