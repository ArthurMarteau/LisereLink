using Lisere.Domain.Enums;
using Lisere.StockApi.Domain.Enums;

namespace Lisere.StockApi.Application.DTOs;

public class StockEntryDto
{
    public Guid ArticleId { get; set; }

    public Size Size { get; set; }

    public int AvailableQuantity { get; set; }

    public StoreType StoreType { get; set; }

    public string StoreId { get; set; } = string.Empty;

    public DateTime LastUpdatedAt { get; set; }
}
