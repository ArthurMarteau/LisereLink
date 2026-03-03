namespace Lisere.Application.DTOs;

public class RequestDto
{
    public Guid Id { get; set; }

    public Guid SellerId { get; set; }

    public Guid? StockistId { get; set; }

    /// <summary>ZoneType converti en string.</summary>
    public string Zone { get; set; } = string.Empty;

    /// <summary>RequestStatus converti en string.</summary>
    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public List<RequestLineDto> Lines { get; set; } = new();
}
