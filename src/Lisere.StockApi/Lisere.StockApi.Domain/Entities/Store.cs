using Lisere.StockApi.Domain.Enums;

namespace Lisere.StockApi.Domain.Entities;

public class Store
{
    public Guid Id { get; set; }

    /// <summary>
    /// Identifiant métier du magasin (slug, ex: "paris-opera").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public StoreType Type { get; set; }
}
