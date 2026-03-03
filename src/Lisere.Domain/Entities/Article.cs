using Lisere.Domain.Enums;

namespace Lisere.Domain.Entities;

public class Article : BaseEntity
{
    public string Barcode { get; set; } = string.Empty;

    public ClothingFamily Family { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ColorOrPrint { get; set; } = string.Empty;

    public List<Size> AvailableSizes { get; set; } = new();

    public decimal? Price { get; set; }

    public string? ImageUrl { get; set; }
}
