using Lisere.Domain.Enums;

namespace Lisere.Domain.Entities;

public class RequestLine : BaseEntity
{
    public Guid RequestId { get; set; }

    public Guid ArticleId { get; set; }

    public string ColorOrPrint { get; set; } = string.Empty;

    public List<Size> RequestedSizes { get; set; } = new();

    public int Quantity { get; set; }

    public RequestLineStatus Status { get; set; }

    // Navigation properties
    public Request Request { get; set; } = null!;

    public Article Article { get; set; } = null!;
}
