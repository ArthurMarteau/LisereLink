using Lisere.Application.DTOs;
using Lisere.Domain.Entities;
using Lisere.Domain.Enums;

namespace Lisere.Application.Mapping;

public static class RequestLineMappingExtensions
{
    public static RequestLineDto ToDto(this RequestLine line) => new()
    {
        Id             = line.Id,
        RequestId      = line.RequestId,
        ArticleId      = line.ArticleId,
        ColorOrPrint   = line.ColorOrPrint,
        RequestedSizes = line.RequestedSizes.Select(s => s.ToString()).ToList(),
        Quantity       = line.Quantity,
        Status         = line.Status.ToString(),
    };

    public static RequestLine ToEntity(this CreateRequestLineDto dto) => new()
    {
        Id             = Guid.NewGuid(),
        ArticleId      = dto.ArticleId,
        ColorOrPrint   = dto.ColorOrPrint,
        RequestedSizes = dto.RequestedSizes.ToList(),
        Quantity       = dto.Quantity,
        Status         = RequestLineStatus.Pending,
    };
}
