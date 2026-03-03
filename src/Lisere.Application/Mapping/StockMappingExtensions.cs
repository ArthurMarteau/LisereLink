using Lisere.Application.DTOs;
using Lisere.Domain.Entities;

namespace Lisere.Application.Mapping;

public static class StockMappingExtensions
{
    public static StockDto ToDto(this Stock stock) => new()
    {
        ArticleId         = stock.ArticleId,
        Size              = stock.Size,
        StoreId           = null,
        AvailableQuantity = stock.AvailableQuantity,
    };

    public static Stock ToEntity(this StockDto dto) => new()
    {
        ArticleId         = dto.ArticleId,
        Size              = dto.Size,
        AvailableQuantity = dto.AvailableQuantity,
    };
}
