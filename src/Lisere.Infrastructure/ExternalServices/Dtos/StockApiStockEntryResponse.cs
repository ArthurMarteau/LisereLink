namespace Lisere.Infrastructure.ExternalServices.Dtos;

internal sealed record StockApiStockEntryResponse(
    Guid ArticleId,
    Guid StoreId,
    string Size,
    int AvailableQuantity);
