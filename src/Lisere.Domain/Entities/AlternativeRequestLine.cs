using Lisere.Domain.Enums;

namespace Lisere.Domain.Entities;

public class AlternativeRequestLine : BaseEntity
{
    public Guid RequestId { get; set; }

    public Guid ArticleId { get; set; }

    public string ArticleName { get; set; } = string.Empty;

    public string ArticleColorOrPrint { get; set; } = string.Empty;

    public string ArticleBarcode { get; set; } = string.Empty;

    public List<string> RequestedSizes { get; set; } = new();

    public int Quantity { get; set; }

    public RequestLineStatus Status { get; set; } // AlternativeProposed | AlternativeDenied | Found

    public bool StockOverride { get; set; }

    // Navigation properties
    public Request Request { get; set; } = null!;
}
