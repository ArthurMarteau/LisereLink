namespace Lisere.Infrastructure.ExternalServices.Dtos;

internal sealed record StockApiStoreResponse(
    Guid Id,
    string Name,
    string Code);
