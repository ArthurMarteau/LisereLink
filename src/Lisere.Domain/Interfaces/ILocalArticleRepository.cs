using Lisere.Domain.Entities;

namespace Lisere.Domain.Interfaces;

public interface ILocalArticleRepository
{
    Task<Article?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IEnumerable<Article> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task AddAsync(Article article, CancellationToken cancellationToken = default);

    Task UpdateAsync(Article article, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Article?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
}
