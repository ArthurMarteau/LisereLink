using Lisere.Application.Common;
using Lisere.Application.DTOs;
using Lisere.Application.Interfaces;
using Lisere.Application.Mapping;
using Lisere.Domain.Enums;
using Lisere.Domain.Interfaces;

namespace Lisere.Application.Services;

public class ArticleService : IArticleService
{
    private readonly ILocalArticleRepository _articleRepository;

    public ArticleService(ILocalArticleRepository articleRepository)
    {
        _articleRepository = articleRepository;
    }

    public async Task<PagedResult<ArticleDto>> SearchAsync(
        string? query,
        ClothingFamily? family,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var (items, totalCount) = await _articleRepository.SearchAsync(query, family, page, pageSize, cancellationToken);

        return new PagedResult<ArticleDto>
        {
            Items = items.Select(a => a.ToDto()),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<ArticleDto?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        var article = await _articleRepository.GetByBarcodeAsync(barcode, cancellationToken);
        return article?.ToDto();
    }
}
