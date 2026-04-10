using Lisere.Domain.Entities;
using Lisere.Domain.Enums;

namespace Lisere.Domain.Interfaces;

public interface IRequestRepository
{
    Task<Request?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IEnumerable<Request> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? storeId = null,
        string? zone = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Request request, CancellationToken cancellationToken = default);

    Task UpdateAsync(Request request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Request>> GetPendingByZoneAsync(ZoneType zone, CancellationToken cancellationToken = default);

    Task<IEnumerable<Request>> GetExpiredPendingAsync(DateTime threshold, CancellationToken cancellationToken = default);
}
