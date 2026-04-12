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
        ArticleName    = line.ArticleName,
        ColorOrPrint   = line.ArticleColorOrPrint,
        ArticleBarcode = line.ArticleBarcode,
        RequestedSizes = line.RequestedSizes.ToList(),
        Quantity       = line.Quantity,
        Status         = line.Status.ToString(),
    };

    public static RequestLine ToEntity(this CreateRequestLineDto dto) => new()
    {
        Id                  = Guid.NewGuid(),
        ArticleId           = dto.ArticleId,
        ArticleName         = dto.ArticleName,
        ArticleColorOrPrint = dto.ArticleColorOrPrint,
        ArticleBarcode      = dto.ArticleBarcode,
        RequestedSizes      = dto.RequestedSizes.ToList(),
        Quantity            = dto.Quantity,
        Status              = RequestLineStatus.Pending,
    };
}
