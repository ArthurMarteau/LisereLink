namespace Lisere.StockApi.Application.DTOs;

public class ArticleDto
{
    public Guid Id { get; set; }

    public string Barcode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Family { get; set; } = string.Empty;

    public string ColorOrPrint { get; set; } = string.Empty;

    public List<string> AvailableSizes { get; set; } = new();
}
