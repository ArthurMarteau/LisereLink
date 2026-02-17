using Lisere.Domain.Enums;

namespace Lisere.Domain.Entities;

public class Stock
{
    public Guid ArticleId { get; set; }

    public Size Size { get; set; }

    public int AvailableQuantity { get; set; }

    // Navigation property
    public Article Article { get; set; } = null!;
}
