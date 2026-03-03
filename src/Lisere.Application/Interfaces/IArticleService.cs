using Lisere.Application.Common;
using Lisere.Application.DTOs;
using Lisere.Domain.Enums;

namespace Lisere.Application.Interfaces;

public interface IArticleService
{
    Task<PagedResult<ArticleDto>> SearchAsync(
        string? query,
        ClothingFamily? family,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ArticleDto?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
}
