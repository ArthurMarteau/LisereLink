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
            .Include(r => r.AlternativeLines)
            .Include(r => r.Seller)
            .Include(r => r.Stockist)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<(IEnumerable<Request> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? storeId = null,
        string? zone = null,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var query = _context.Requests
            .Include(r => r.Lines)
            .Include(r => r.AlternativeLines)
            .Include(r => r.Seller)
            .Include(r => r.Stockist)
            .AsQueryable();

        if (!string.IsNullOrEmpty(storeId))
            query = query.Where(r => r.StoreId == storeId);

        if (!string.IsNullOrEmpty(zone) && Enum.TryParse<ZoneType>(zone, ignoreCase: true, out var zoneType))
            query = query.Where(r => r.Zone == zoneType);

        query = query.OrderByDescending(r => r.CreatedAt);

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
        // L'entité est déjà trackée par le même DbContext (chargée via GetByIdAsync)
        // EF Core détecte automatiquement les changements — SaveChanges suffit
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
            .Include(r => r.AlternativeLines)
            .Include(r => r.Seller)
            .Where(r => r.Zone == zone && r.Status == RequestStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Request>> GetExpiredPendingAsync(
        DateTime threshold,
        CancellationToken cancellationToken = default)
    {
        return await _context.Requests
            .Where(r => r.Status == RequestStatus.Pending && r.CreatedAt <= threshold)
            .ToListAsync(cancellationToken);
    }
}
