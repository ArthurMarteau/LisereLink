using Lisere.Domain.Entities;
using Lisere.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lisere.Infrastructure.Persistence.Repositories;

public class RequestLineRepository : IRequestLineRepository
{
    private readonly LisereDbContext _context;

    public RequestLineRepository(LisereDbContext context)
    {
        _context = context;
    }

    public async Task<RequestLine?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.RequestLines
            .Include(rl => rl.Request)
                .ThenInclude(r => r.Seller)
            .FirstOrDefaultAsync(rl => rl.Id == id, cancellationToken);
    }

    public async Task<(IEnumerable<RequestLine> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var query = _context.RequestLines
            .Include(rl => rl.Request)
            .OrderByDescending(rl => rl.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(RequestLine requestLine, CancellationToken cancellationToken = default)
    {
        await _context.RequestLines.AddAsync(requestLine, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(RequestLine requestLine, CancellationToken cancellationToken = default)
    {
        _context.RequestLines.Update(requestLine);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var requestLine = await _context.RequestLines
            .FirstOrDefaultAsync(rl => rl.Id == id, cancellationToken);

        if (requestLine is null)
            return;

        requestLine.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<RequestLine>> GetByRequestIdAsync(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        return await _context.RequestLines
            .Where(rl => rl.RequestId == requestId)
            .OrderBy(rl => rl.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
