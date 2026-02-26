namespace Lisere.Application.DTOs;

public class StockDto
{
    public Guid ArticleId { get; set; }

    /// <summary>Size converti en string.</summary>
    public string Size { get; set; } = string.Empty;

    /// <summary>Identifiant du magasin. Nullable : l'entité Stock locale ne porte pas de StoreId.</summary>
    public string? StoreId { get; set; }

    public int AvailableQuantity { get; set; }
}
