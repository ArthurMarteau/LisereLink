using Lisere.Application.DTOs;
using Lisere.Domain.Entities;
using Lisere.Infrastructure.ExternalServices.Dtos;

namespace Lisere.Infrastructure.ExternalServices.Mappers;

internal static class StockApiMapper
{
    public static ArticleDto MapToArticleDto(this StockApiArticleResponse r) => new()
    {
        Id           = r.Id,
        Barcode      = r.Barcode,
        Name         = r.Name,
        Family       = r.Family,
        ColorOrPrint = r.ColorOrPrint,
        AvailableSizes = r.AvailableSizes.ToList(),
        Price        = r.Price,
        ImageUrl     = r.ImageUrl,
    };

    public static Stock MapToStock(this StockApiStockEntryResponse s) => new()
    {
        ArticleId         = s.ArticleId,
        Size              = s.Size,
        AvailableQuantity = s.AvailableQuantity,
    };
}
