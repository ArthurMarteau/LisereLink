namespace Lisere.Infrastructure.ExternalServices.Dtos;

internal sealed record StockApiStockEntryResponse
{
    public Guid ArticleId { get; init; }
    public string StoreId { get; init; } = string.Empty;
    public string Size { get; init; } = string.Empty;
    public int AvailableQuantity { get; init; }
}
