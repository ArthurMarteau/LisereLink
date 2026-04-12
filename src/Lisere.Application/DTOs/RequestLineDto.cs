namespace Lisere.Application.DTOs;

public class RequestLineDto
{
    public Guid Id { get; set; }

    public Guid RequestId { get; set; }

    public Guid ArticleId { get; set; }

    public string ArticleName { get; set; } = string.Empty;

    public string ColorOrPrint { get; set; } = string.Empty;

    /// <summary>Liste des tailles demandées converties en string.</summary>
    public List<string> RequestedSizes { get; set; } = new();

    public int Quantity { get; set; }

    public string ArticleBarcode { get; set; } = string.Empty;

    /// <summary>RequestLineStatus converti en string.</summary>
    public string Status { get; set; } = string.Empty;
}
