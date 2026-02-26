using Lisere.Application.DTOs;
using Lisere.Domain.Entities;
using Lisere.Domain.Enums;

namespace Lisere.Application.Mapping;

public static class ArticleMappingExtensions
{
    public static ArticleDto ToDto(this Article article) => new()
    {
        Id            = article.Id,
        Barcode       = article.Barcode,
        Name          = article.Name,
        Family        = article.Family.ToString(),
        ColorOrPrint  = article.ColorOrPrint,
        AvailableSizes = article.AvailableSizes.Select(s => s.ToString()).ToList(),
        Price         = article.Price,
        ImageUrl      = article.ImageUrl,
        LastSyncedAt  = article.ModifiedAt,
    };

    public static Article ToEntity(this ArticleDto dto) => new()
    {
        Id            = dto.Id,
        Barcode       = dto.Barcode,
        Name          = dto.Name,
        Family        = Enum.Parse<ClothingFamily>(dto.Family),
        ColorOrPrint  = dto.ColorOrPrint,
        AvailableSizes = dto.AvailableSizes
                            .Select(s => Enum.Parse<Size>(s))
                            .ToList(),
        Price         = dto.Price,
        ImageUrl      = dto.ImageUrl,
    };
}
