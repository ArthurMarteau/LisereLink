using Lisere.StockApi.Domain.Enums;

namespace Lisere.StockApi.Domain.Entities;

/// <summary>
/// Source de vérité pour les articles — appartient à Lisere.StockApi.
/// Pas de BaseEntity : suppression physique, LastUpdatedAt uniquement (pas d'audit trail complet).
/// </summary>
public class 
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    Article
{
    public Guid Id { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public ClothingFamily Family { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ColorOrPrint { get; set; } = string.Empty;
    public List<Size> AvailableSizes { get; set; } = new();
    public decimal? Price { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}
