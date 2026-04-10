using Lisere.Application.DTOs;
using Lisere.Domain.Entities;
using Lisere.Domain.Enums;

namespace Lisere.Application.Mapping;

public static class RequestMappingExtensions
{
    public static RequestDto ToDto(this Request request) => new()
    {
        Id          = request.Id,
        SellerId    = request.SellerId,
        StockistId  = request.StockistId,
        StoreId     = request.StoreId,
        Zone        = request.Zone.ToString(),
        Status      = request.Status.ToString(),
        CreatedAt   = request.CreatedAt,
        CompletedAt = request.CompletedAt,
        Lines       = request.Lines.Select(l => l.ToDto()).ToList(),
    };

    public static Request ToEntity(this CreateRequestDto dto) => new()
    {
        Id       = Guid.NewGuid(),
        SellerId = dto.SellerId,
        StoreId  = dto.StoreId,
        Zone     = dto.Zone,
        Status   = RequestStatus.Pending,
        Lines    = dto.Lines.Select(l => l.ToEntity()).ToList(),
    };
}
