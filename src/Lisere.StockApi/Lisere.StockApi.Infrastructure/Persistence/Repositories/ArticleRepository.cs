using Lisere.StockApi.Domain.Entities;
using Lisere.StockApi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lisere.StockApi.Infrastructure.Persistence.Repositories;

public class ArticleRepository : IArticleRepository
{
    private readonly StockApiDbContext _context;

    public ArticleRepository(StockApiDbContext context)
    {
        _context = context;
    }

    public async Task<Article?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
    
    public async Task<IEnumerable<Article>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _context.Articles
            .Where(a => ids.Contains(a.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Article?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        return await _context.Articles
            .FirstOrDefaultAsync(a => a.Barcode == barcode, cancellationToken);
    }

    public async Task<(IEnumerable<Article> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? query = null,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var dbQuery = _context.Articles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lower = query.ToLower();
            dbQuery = dbQuery.Where(a =>
                a.Name.ToLower().Contains(lower) ||
                a.ColorOrPrint.ToLower().Contains(lower));
        }

        dbQuery = dbQuery
            .OrderBy(a => a.Family)
            .ThenBy(a => a.Name);

        var totalCount = await dbQuery.CountAsync(cancellationToken);
        var items = await dbQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Article article, CancellationToken cancellationToken = default)
    {
        await _context.Articles.AddAsync(article, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Article article, CancellationToken cancellationToken = default)
    {
        _context.Articles.Update(article);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (article is null)
            return;

        // Suppression physique — StockApi est la source de vérité, pas de soft delete
        _context.Articles.Remove(article);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
