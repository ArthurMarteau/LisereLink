using Lisere.Application.DTOs;
using Lisere.Domain.Entities;
using Lisere.Domain.Enums;

namespace Lisere.Application.Mapping;

public static class StockMappingExtensions
{
    /// <summary>
    /// Mappe Stock → StockDto.
    /// StoreId reste null : l'entité Stock locale ne porte pas d'identifiant de magasin.
    /// La couche service peut renseigner StoreId selon le contexte d'appel.
    /// </summary>
    public static StockDto ToDto(this Stock stock) => new()
    {
        ArticleId         = stock.ArticleId,
        Size              = stock.Size.ToString(),
        StoreId           = null,
        AvailableQuantity = stock.AvailableQuantity,
    };

    public static Stock ToEntity(this StockDto dto) => new()
    {
        ArticleId         = dto.ArticleId,
        Size              = Enum.Parse<Size>(dto.Size),
        AvailableQuantity = dto.AvailableQuantity,
    };
}
