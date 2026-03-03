using Lisere.StockApi.Application.Common;
using Lisere.StockApi.Application.DTOs;
using Lisere.StockApi.Application.Exceptions;
using Lisere.StockApi.Application.Interfaces;
using Lisere.StockApi.Domain.Entities;
using Lisere.StockApi.Domain.Enums;
using Lisere.StockApi.Domain.Interfaces;

namespace Lisere.StockApi.Application.Services;

public class StockService : IStockService
{
    private readonly IStockEntryRepository _stockEntryRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly IArticleRepository _articleRepository;

    public StockService(
        IStockEntryRepository stockEntryRepository,
        IStoreRepository storeRepository,
        IArticleRepository articleRepository)
    {
        _stockEntryRepository = stockEntryRepository;
        _storeRepository = storeRepository;
        _articleRepository = articleRepository;
    }

    public async Task<IEnumerable<StockEntryDto>> GetStockAsync(
        Guid articleId,
        string storeId,
        CancellationToken cancellationToken = default)
    {
        var entries = await _stockEntryRepository.GetByArticleAsync(articleId, storeId, cancellationToken);
        return entries.Select(MapEntryToDto);
    }

    public async Task<PagedResult<ArticleStockDto>> GetAllArticlesWithStockAsync(
        string storeId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var (entriesList, totalCount) = await _stockEntryRepository.GetByStoreAsync(storeId, page, pageSize, cancellationToken);
        var entries = entriesList.ToList();

        // Fetch article metadata for each distinct article in this page
        var articleIds = entries.Select(e => e.ArticleId).Distinct().ToList();
        var articleList = await _articleRepository.GetByIdsAsync(articleIds, cancellationToken);
        var articles = articleList.ToDictionary(a => a.Id);

        var grouped = entries
            .GroupBy(e => e.ArticleId)
            .Select(g =>
            {
                articles.TryGetValue(g.Key, out var article);
                return new ArticleStockDto
                {
                    ArticleId = g.Key,
                    Barcode = article?.Barcode ?? string.Empty,
                    Name = article?.Name ?? string.Empty,
                    Family = article?.Family.ToString() ?? string.Empty,
                    ColorOrPrint = article?.ColorOrPrint ?? string.Empty,
                    Stock = g.Select(MapEntryToDto).ToList()
                };
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
            throw new StockException("La quantité ne peut pas être négative.");

        var article = await _articleRepository.GetByIdAsync(dto.ArticleId, cancellationToken);
        if (article is null)
            throw new StockException($"Article introuvable : {dto.ArticleId}.");

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

    public async Task<PagedResult<ArticleDto>> GetArticlesAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var (items, totalCount) = await _articleRepository.GetAllAsync(page, pageSize, cancellationToken);

        return new PagedResult<ArticleDto>
        {
            Items = items.Select(MapArticleToDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ArticleDto?> GetArticleByBarcodeAsync(
        string barcode,
        CancellationToken cancellationToken = default)
    {
        var article = await _articleRepository.GetByBarcodeAsync(barcode, cancellationToken);
        return article is null ? null : MapArticleToDto(article);
    }

    private static StockEntryDto MapEntryToDto(StockEntry entry) => new()
    {
        ArticleId = entry.ArticleId,
        Size = entry.Size,
        AvailableQuantity = entry.AvailableQuantity,
        StoreType = entry.StoreType,
        StoreId = entry.StoreId,
        LastUpdatedAt = entry.LastUpdatedAt
    };

    private static ArticleDto MapArticleToDto(Article article) => new()
    {
        Id = article.Id,
        Barcode = article.Barcode,
        Name = article.Name,
        Family = article.Family.ToString(),
        ColorOrPrint = article.ColorOrPrint,
        AvailableSizes = article.AvailableSizes.Select(s => s.ToString()).ToList()
    };
}
