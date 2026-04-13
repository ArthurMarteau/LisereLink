using System.ComponentModel.DataAnnotations;

namespace Lisere.Application.DTOs;

public class ProposeAlternativesDto
{
    [Required]
    public Guid StockistId { get; set; }

    [Required, MinLength(1)]
    public List<ProposeAlternativeLineDto> Lines { get; set; } = new();
}

public class ProposeAlternativeLineDto
{
    [Required]
    public Guid ArticleId { get; set; }

    [Required, MaxLength(200)]
    public string ArticleName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string ArticleColorOrPrint { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string ArticleBarcode { get; set; } = string.Empty;

    [Required, MinLength(1)]
    public List<string> RequestedSizes { get; set; } = new();

    [Range(1, 100)]
    public int Quantity { get; set; } = 1;

    public bool StockOverride { get; set; }
}
