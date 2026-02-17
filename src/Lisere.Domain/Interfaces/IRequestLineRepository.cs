using Lisere.Domain.Entities;

namespace Lisere.Domain.Interfaces;

public interface IRequestLineRepository
{
    Task<RequestLine?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IEnumerable<RequestLine> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task AddAsync(RequestLine requestLine, CancellationToken cancellationToken = default);

    Task UpdateAsync(RequestLine requestLine, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<RequestLine>> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default);
}
