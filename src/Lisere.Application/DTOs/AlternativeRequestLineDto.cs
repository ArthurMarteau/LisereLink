namespace Lisere.Application.DTOs;

public class AlternativeRequestLineDto
{
    public Guid Id { get; set; }

    public Guid RequestId { get; set; }

    public Guid ArticleId { get; set; }

    public string ArticleName { get; set; } = string.Empty;

    public string ArticleColorOrPrint { get; set; } = string.Empty;

    public string ArticleBarcode { get; set; } = string.Empty;

    public List<string> RequestedSizes { get; set; } = new();

    public int Quantity { get; set; }

    /// <summary>RequestLineStatus converti en string.</summary>
    public string Status { get; set; } = string.Empty;

    public bool StockOverride { get; set; }
}
