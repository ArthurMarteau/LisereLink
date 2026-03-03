using Lisere.StockApi.Domain.Enums;

namespace Lisere.StockApi.Domain.Entities;

public class StockEntry
{
    public Guid Id { get; set; }

    public Guid ArticleId { get; set; }

    public Size Size { get; set; }

    public int AvailableQuantity { get; set; }

    public StoreType StoreType { get; set; }
    
    public string StoreId { get; set; } = string.Empty;

    public DateTime LastUpdatedAt { get; set; }
}
