using Lisere.Application.DTOs;

namespace Lisere.Application.Interfaces;

public interface INotificationService
{
    Task NotifyNewRequestAsync(RequestDto request);
    Task NotifyRequestUpdatedAsync(RequestDto request);
    Task NotifyRequestCancelledAsync(Guid requestId, Guid sellerId);
}
