using Lisere.Domain.Entities;
using Lisere.Domain.Enums;
using Lisere.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lisere.Infrastructure.Persistence.Repositories;

public class RequestRepository : IRequestRepository
{
    private readonly LisereDbContext _context;

    public RequestRepository(LisereDbContext context)
    {
        _context = context;
    }

    public async Task<Request?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Include(r => r.Lines)
            .Include(r => r.Seller)
            .Include(r => r.Stockist)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<(IEnumerable<Request> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var query = _context.Requests
            .Include(r => r.Lines)
            .Include(r => r.Seller)
            .Include(r => r.Stockist)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Request request, CancellationToken cancellationToken = default)
    {
        await _context.Requests.AddAsync(request, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Request request, CancellationToken cancellationToken = default)
    {
        _context.Requests.Update(request);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await _context.Requests
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (request is null)
            return;

        request.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Request>> GetPendingByZoneAsync(
        ZoneType zone,
        CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Include(r => r.Lines)
            .Include(r => r.Seller)
            .Where(r => r.Zone == zone && r.Status == RequestStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
