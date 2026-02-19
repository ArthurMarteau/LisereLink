using Lisere.StockApi.Domain.Entities;
using Lisere.StockApi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lisere.StockApi.Infrastructure.Persistence.Repositories;

public class StoreRepository : IStoreRepository
{
    private readonly StockApiDbContext _context;

    public StoreRepository(StockApiDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Store>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Stores
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Stores
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Store?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Stores
            .FirstOrDefaultAsync(s => s.Code == code, cancellationToken);
    }
}
