using Lisere.Application.Common;
using Lisere.Application.DTOs;

namespace Lisere.Application.Interfaces;

public interface IArticleService
{
    Task<PagedResult<ArticleDto>> SearchAsync(
        string? query,
        string? family,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ArticleDto?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
}
