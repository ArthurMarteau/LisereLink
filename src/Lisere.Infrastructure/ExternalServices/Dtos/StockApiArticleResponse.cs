namespace Lisere.Infrastructure.ExternalServices.Dtos;

internal sealed record StockApiArticleResponse(
    Guid Id,
    string Barcode,
    string Name,
    string Family,
    string ColorOrPrint,
    List<string> AvailableSizes,
    decimal? Price,
    string? ImageUrl);
