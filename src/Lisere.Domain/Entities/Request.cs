using Lisere.Domain.Enums;

namespace Lisere.Domain.Entities;

public class Request : BaseEntity
{
    public Guid SellerId { get; set; }

    public Guid? StockistId { get; set; }

    public ZoneType Zone { get; set; }

    public RequestStatus Status { get; set; }

    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public List<RequestLine> Lines { get; set; } = new();

    public User Seller { get; set; } = null!;

    public User? Stockist { get; set; }
}
