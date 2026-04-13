namespace Lisere.Domain.Enums;

public enum RequestStatus
{
    Pending,
    InProgress,
    AwaitingSellerResponse,
    Processed,
    PartiallyProcessed,
    Unavailable,
    Cancelled
}
