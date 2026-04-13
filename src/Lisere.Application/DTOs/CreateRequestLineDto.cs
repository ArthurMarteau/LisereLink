using System.ComponentModel.DataAnnotations;

namespace Lisere.Application.DTOs;

public class CreateRequestLineDto
{
    [Required]
    public Guid ArticleId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ArticleName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ArticleColorOrPrint { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string ArticleBarcode { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string Size { get; set; } = string.Empty;

    [Range(1, 100)]
    public int Quantity { get; set; } = 1;
}
