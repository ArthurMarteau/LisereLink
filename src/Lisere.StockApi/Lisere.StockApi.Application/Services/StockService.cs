using Lisere.StockApi.Application.DTOs;
using Lisere.StockApi.Application.Interfaces;
using Lisere.StockApi.Domain.Entities;
using Lisere.StockApi.Domain.Enums;
using Lisere.StockApi.Domain.Interfaces;

namespace Lisere.StockApi.Application.Services;

public class StockService : IStockService
{
    private readonly IStockEntryRepository _stockEntryRepository;
    private readonly IStoreRepository _storeRepository;

    public StockService(IStockEntryRepository stockEntryRepository, IStoreRepository storeRepository)
    {
        _stockEntryRepository = stockEntryRepository;
        _storeRepository = storeRepository;
    }

    public async Task<IEnumerable<StockEntryDto>> GetStockByArticleAsync(
        Guid articleId,
        string storeId,
        CancellationToken cancellationToken = default)
    {
        var entries = await _stockEntryRepository.GetByArticleAndStoreAsync(articleId, storeId, cancellationToken);
        return entries.Select(MapToDto);
    }

    public async Task<PagedResult<ArticleStockDto>> GetStockByStoreAsync(
        string storeId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var (entries, totalCount) = await _stockEntryRepository.GetByStoreAsync(storeId, page, pageSize, cancellationToken);

        var grouped = entries
            .GroupBy(e => e.ArticleId)
            .Select(g => new ArticleStockDto
            {
                ArticleId = g.Key,
                Stock = g.Select(MapToDto).ToList()
            });

        return new PagedResult<ArticleStockDto>
        {
            Items = grouped,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task UpdateStockAsync(UpdateStockDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.NewQuantity < 0)
            throw new ArgumentOutOfRangeException(nameof(dto.NewQuantity), "La quantité ne peut pas être négative.");

        var store = await _storeRepository.GetByCodeAsync(dto.StoreId, cancellationToken);
        var storeType = store?.Type ?? StoreType.Physical;

        var entry = new StockEntry
        {
            Id = Guid.NewGuid(),
            ArticleId = dto.ArticleId,
            Size = dto.Size,
            AvailableQuantity = dto.NewQuantity,
            StoreId = dto.StoreId,
            StoreType = storeType,
            LastUpdatedAt = DateTime.UtcNow
        };

        await _stockEntryRepository.UpsertAsync(entry, cancellationToken);
    }

    private static StockEntryDto MapToDto(StockEntry entry) => new()
    {
        ArticleId = entry.ArticleId,
        Size = entry.Size,
        AvailableQuantity = entry.AvailableQuantity,
        StoreType = entry.StoreType,
        StoreId = entry.StoreId,
        LastUpdatedAt = entry.LastUpdatedAt
    };
}
