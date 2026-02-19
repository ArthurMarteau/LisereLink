using Lisere.StockApi.Domain.Enums;

namespace Lisere.StockApi.Application.DTOs;

public class StoreDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public StoreType Type { get; set; }
}
