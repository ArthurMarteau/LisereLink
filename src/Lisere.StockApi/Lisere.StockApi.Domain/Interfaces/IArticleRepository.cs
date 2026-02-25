using Lisere.Domain.Entities;

namespace Lisere.StockApi.Domain.Interfaces;

/// <summary>
/// Repository articles côté StockApi — source de vérité, CRUD complet.
/// Distinct de Lisere.Domain.Interfaces.ILocalArticleRepository (lecture seule dans Lisere.API).
/// </summary>
public interface IArticleRepository
{
    Task<Article?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Article?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);

    Task<(IEnumerable<Article> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Article>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    Task AddAsync(Article article, CancellationToken cancellationToken = default);

    Task UpdateAsync(Article article, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
