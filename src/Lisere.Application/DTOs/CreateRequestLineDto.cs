using System.ComponentModel.DataAnnotations;
using Lisere.Domain.Enums;

namespace Lisere.Application.DTOs;

public class CreateRequestLineDto
{
    [Required]
    public Guid ArticleId { get; set; }

    [Required]
    [MaxLength(200)]
    public string ColorOrPrint { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<Size> RequestedSizes { get; set; } = new();

    [Range(1, 100)]
    public int Quantity { get; set; } = 1;
}
