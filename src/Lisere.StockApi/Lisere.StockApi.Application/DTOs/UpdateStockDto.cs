using System.ComponentModel.DataAnnotations;
using Lisere.StockApi.Domain.Enums;

namespace Lisere.StockApi.Application.DTOs;

public class UpdateStockDto
{
    [Required]
    public Guid ArticleId { get; set; }

    [Required]
    public Size Size { get; set; }

    [Required]
    [MinLength(1)]
    public string StoreId { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "La quantité ne peut pas être négative.")]
    public int NewQuantity { get; set; }
}
