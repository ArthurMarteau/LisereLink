namespace Lisere.Application.DTOs;

public class ArticleDto
{
    public Guid Id { get; set; }

    public string Barcode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>ClothingFamily converti en string.</summary>
    public string Family { get; set; } = string.Empty;

    public string ColorOrPrint { get; set; } = string.Empty;

    /// <summary>Liste des tailles disponibles converties en string.</summary>
    public List<string> AvailableSizes { get; set; } = new();

    public decimal? Price { get; set; }

    public string? ImageUrl { get; set; }

    /// <summary>Correspond à Article.ModifiedAt — dernière synchronisation depuis le StockApi.</summary>
    public DateTime? LastSyncedAt { get; set; }
}
