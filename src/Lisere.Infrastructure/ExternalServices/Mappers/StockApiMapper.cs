using Lisere.Application.DTOs;
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

    public static StockDto MapToStockDto(this StockApiStockEntryResponse s) => new()
    {
        ArticleId         = s.ArticleId,
        Size              = s.Size,
        StoreId           = s.StoreId.ToString(),
        AvailableQuantity = s.AvailableQuantity,
    };
}
