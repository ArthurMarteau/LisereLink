using Lisere.Domain.Entities;

namespace Lisere.Domain.Interfaces;

public interface IExternalStockApiClient
{
    Task<IEnumerable<Article>> GetArticlesAsync(CancellationToken cancellationToken = default);

    Task<Article?> GetArticleByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);

    Task<IEnumerable<Stock>> GetStockAsync(Guid articleId, CancellationToken cancellationToken = default);
}
