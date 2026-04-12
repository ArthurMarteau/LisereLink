namespace Lisere.Application.DTOs;

public class RequestDto
{
    public Guid Id { get; set; }

    public Guid SellerId { get; set; }

    public string SellerFirstName { get; set; } = string.Empty;

    public string SellerLastName { get; set; } = string.Empty;

    public Guid? StockistId { get; set; }

    public string? StockistFirstName { get; set; }

    public string? StockistLastName { get; set; }

    public string StoreId { get; set; } = string.Empty;

    /// <summary>ZoneType converti en string.</summary>
    public string Zone { get; set; } = string.Empty;

    /// <summary>RequestStatus converti en string.</summary>
    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public List<RequestLineDto> Lines { get; set; } = new();
}
