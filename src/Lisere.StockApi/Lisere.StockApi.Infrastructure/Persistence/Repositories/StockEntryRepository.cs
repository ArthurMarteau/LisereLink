using Lisere.Domain.Enums;
using Lisere.StockApi.Domain.Entities;
using Lisere.StockApi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lisere.StockApi.Infrastructure.Persistence.Repositories;

public class StockEntryRepository : IStockEntryRepository
{
    private readonly StockApiDbContext _context;

    public StockEntryRepository(StockApiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StockEntry>> GetByArticleAsync(
        Guid articleId,
        string storeId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StockEntries
            .Where(se => se.ArticleId == articleId && se.StoreId == storeId)
            .OrderBy(se => se.Size)
            .ToListAsync(cancellationToken);
    }

    public async Task<StockEntry?> GetByArticleAndSizeAsync(
        Guid articleId,
        Size size,
        string storeId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StockEntries
            .FirstOrDefaultAsync(se =>
                se.ArticleId == articleId &&
                se.Size == size &&
                se.StoreId == storeId,
                cancellationToken);
    }

    public async Task<(IEnumerable<StockEntry> Items, int TotalCount)> GetByStoreAsync(
        string storeId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var query = _context.StockEntries
            .Where(se => se.StoreId == storeId)
            .OrderBy(se => se.ArticleId)
            .ThenBy(se => se.Size);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task UpsertAsync(StockEntry entry, CancellationToken cancellationToken = default)
    {
        var existing = await _context.StockEntries
            .FirstOrDefaultAsync(se =>
                se.ArticleId == entry.ArticleId &&
                se.Size == entry.Size &&
                se.StoreId == entry.StoreId,
                cancellationToken);

        if (existing is null)
        {
            await _context.StockEntries.AddAsync(entry, cancellationToken);
        }
        else
        {
            existing.AvailableQuantity = entry.AvailableQuantity;
            existing.LastUpdatedAt = entry.LastUpdatedAt;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertRangeAsync(
        IEnumerable<StockEntry> entries,
        CancellationToken cancellationToken = default)
    {
        foreach (var entry in entries)
        {
            var existing = await _context.StockEntries
                .FirstOrDefaultAsync(se =>
                    se.ArticleId == entry.ArticleId &&
                    se.Size == entry.Size &&
                    se.StoreId == entry.StoreId,
                    cancellationToken);

            if (existing is null)
            {
                await _context.StockEntries.AddAsync(entry, cancellationToken);
            }
            else
            {
                existing.AvailableQuantity = entry.AvailableQuantity;
                existing.LastUpdatedAt = entry.LastUpdatedAt;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
