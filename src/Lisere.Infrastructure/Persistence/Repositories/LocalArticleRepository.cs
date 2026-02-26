using Lisere.Domain.Entities;
using Lisere.Domain.Enums;
using Lisere.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lisere.Infrastructure.Persistence.Repositories;

public class LocalArticleRepository : ILocalArticleRepository
{
    private readonly LisereDbContext _context;

    public LocalArticleRepository(LisereDbContext context)
    {
        _context = context;
    }

    public async Task<Article?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Articles
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<(IEnumerable<Article> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var query = _context.Articles
            .OrderBy(a => a.Family)
            .ThenBy(a => a.Name);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IEnumerable<Article> Items, int TotalCount)> SearchAsync(
        string? query,
        ClothingFamily? family,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var q = _context.Articles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lower = query.ToLower();
            q = q.Where(a =>
                a.Name.ToLower().Contains(lower) ||
                a.Barcode.Contains(query) ||
                a.ColorOrPrint.ToLower().Contains(lower));
        }

        if (family.HasValue)
            q = q.Where(a => a.Family == family.Value);

        q = q.OrderBy(a => a.Family).ThenBy(a => a.Name);

        var totalCount = await q.CountAsync(cancellationToken);
        var items = await q
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

        article.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Article?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        return await _context.Articles
            .FirstOrDefaultAsync(a => a.Barcode == barcode, cancellationToken);
    }
}