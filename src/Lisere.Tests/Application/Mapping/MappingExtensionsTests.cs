using Lisere.Application.DTOs;
using Lisere.Application.Mapping;
using Lisere.Domain.Entities;
using Lisere.Domain.Enums;
using Xunit;

namespace Lisere.Tests.Application.Mapping;

public class MappingExtensionsTests
{
    // ── Request → RequestDto ─────────────────────────────────────────────────

    [Fact]
    public void RequestToDto_MapsAllFields()
    {
        var request = new Request
        {
            Id          = Guid.NewGuid(),
            SellerId    = Guid.NewGuid(),
            StockistId  = Guid.NewGuid(),
            Zone        = ZoneType.RTW,
            Status      = RequestStatus.InProgress,
            CreatedAt   = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            CompletedAt = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            Lines       = [],
            CreatedBy   = "user1",
        };

        var dto = request.ToDto();

        Assert.Equal(request.Id, dto.Id);
        Assert.Equal(request.SellerId, dto.SellerId);
        Assert.Equal(request.StockistId, dto.StockistId);
        Assert.Equal("RTW", dto.Zone);
        Assert.Equal("InProgress", dto.Status);
        Assert.Equal(request.CreatedAt, dto.CreatedAt);
        Assert.Equal(request.CompletedAt, dto.CompletedAt);
        Assert.Empty(dto.Lines);
    }

    [Fact]
    public void RequestToDto_WithNullOptionals_MapsCorrectly()
    {
        var request = new Request
        {
            Id        = Guid.NewGuid(),
            SellerId  = Guid.NewGuid(),
            Zone      = ZoneType.Checkout,
            Status    = RequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Lines     = [],
            CreatedBy = "user2",
        };

        var dto = request.ToDto();

        Assert.Null(dto.StockistId);
        Assert.Null(dto.CompletedAt);
    }

    // ── CreateRequestDto → Request ───────────────────────────────────────────

    [Fact]
    public void CreateRequestDtoToEntity_MapsAllFields()
    {
        var dto = new CreateRequestDto
        {
            SellerId = Guid.NewGuid(),
            Zone     = ZoneType.FittingRooms,
            Lines    = [],
        };

        var entity = dto.ToEntity();

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(dto.SellerId, entity.SellerId);
        Assert.Equal(ZoneType.FittingRooms, entity.Zone);
        Assert.Equal(RequestStatus.Pending, entity.Status);
        Assert.Empty(entity.Lines);
    }

    // ── RequestLine → RequestLineDto ─────────────────────────────────────────

    [Fact]
    public void RequestLineToDto_MapsAllFields()
    {
        var line = new RequestLine
        {
            Id                  = Guid.NewGuid(),
            RequestId           = Guid.NewGuid(),
            ArticleId           = Guid.NewGuid(),
            ArticleColorOrPrint = "Bleu nuit",
            RequestedSizes      = ["S", "M"],
            Quantity            = 2,
            Status              = RequestLineStatus.Found,
            CreatedBy           = "user3",
        };

        var dto = line.ToDto();

        Assert.Equal(line.Id, dto.Id);
        Assert.Equal(line.RequestId, dto.RequestId);
        Assert.Equal(line.ArticleId, dto.ArticleId);
        Assert.Equal("Bleu nuit", dto.ColorOrPrint);
        Assert.Equal(2, dto.RequestedSizes.Count);
        Assert.Equal(2, dto.Quantity);
        Assert.Equal("Found", dto.Status);
    }

    // ── CreateRequestLineDto → RequestLine ───────────────────────────────────

    [Fact]
    public void CreateRequestLineDtoToEntity_MapsAllFields()
    {
        var dto = new CreateRequestLineDto
        {
            ArticleId           = Guid.NewGuid(),
            ArticleName         = "Manteau Laine",
            ArticleColorOrPrint = "Rouge",
            ArticleBarcode      = "1234567890123",
            RequestedSizes      = ["L", "XL"],
            Quantity            = 3,
        };

        var entity = dto.ToEntity();

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(dto.ArticleId, entity.ArticleId);
        Assert.Equal("Manteau Laine", entity.ArticleName);
        Assert.Equal("Rouge", entity.ArticleColorOrPrint);
        Assert.Equal("1234567890123", entity.ArticleBarcode);
        Assert.Equal(2, entity.RequestedSizes.Count);
        Assert.Equal(3, entity.Quantity);
        Assert.Equal(RequestLineStatus.Pending, entity.Status);
    }
}
