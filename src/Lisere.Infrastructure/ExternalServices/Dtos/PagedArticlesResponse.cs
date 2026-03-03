namespace Lisere.Infrastructure.ExternalServices.Dtos;

internal sealed record PagedArticlesResponse(
    IEnumerable<StockApiArticleResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);
