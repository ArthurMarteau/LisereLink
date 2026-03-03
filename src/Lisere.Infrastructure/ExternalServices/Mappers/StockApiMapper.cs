using Lisere.Domain.Entities;
using Lisere.Domain.Enums;
using Lisere.Infrastructure.ExternalServices.Dtos;

namespace Lisere.Infrastructure.ExternalServices.Mappers;

internal static class StockApiMapper
{
    public static Article MapToArticle(this StockApiArticleResponse r) => new()
    {
        Id = r.Id,
        Barcode = r.Barcode,
        Name = r.Name,
        Family = Enum.Parse<ClothingFamily>(r.Family),
        ColorOrPrint = r.ColorOrPrint,
        AvailableSizes = r.AvailableSizes
            .Select(s => Enum.Parse<Size>(s))
            .ToList(),
        Price = r.Price,
        ImageUrl = r.ImageUrl,
    };

    public static Stock MapToStock(this StockApiStockEntryResponse s) => new()
    {
        ArticleId = s.ArticleId,
        Size = Enum.Parse<Size>(s.Size),
        AvailableQuantity = s.AvailableQuantity,
    };
}
