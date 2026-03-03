namespace Lisere.Domain.Entities;

public class Stock
{
    public Guid ArticleId { get; set; }

    public string Size { get; set; } = string.Empty;

    public int AvailableQuantity { get; set; }
}
