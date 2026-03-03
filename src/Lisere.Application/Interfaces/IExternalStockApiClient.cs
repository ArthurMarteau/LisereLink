using Lisere.Application.Common;
using Lisere.Application.DTOs;
using Lisere.Domain.Entities;

namespace Lisere.Application.Interfaces;

public interface IExternalStockApiClient
{
    Task<IEnumerable<Stock>> GetStockAsync(Guid articleId, Guid storeId, CancellationToken cancellationToken = default);

    Task<PagedResult<ArticleDto>> SearchArticlesAsync(
        string? query,
        string? family,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ArticleDto?> GetArticleByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
}
