namespace Lisere.StockApi.Application.DTOs;

public class ArticleStockDto
{
    public Guid ArticleId { get; set; }

    public string Barcode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Family { get; set; } = string.Empty;

    public string ColorOrPrint { get; set; } = string.Empty;

    public List<StockEntryDto> Stock { get; set; } = new();
}
