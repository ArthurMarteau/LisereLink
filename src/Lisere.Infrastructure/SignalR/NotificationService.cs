using Lisere.Application.DTOs;
using Lisere.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Lisere.Infrastructure.SignalR;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyNewRequestAsync(RequestDto request)
    {
        var group = $"zone-{request.Zone}";
        await _hubContext.Clients.Group(group).SendAsync("NewRequest", request);
    }

    public async Task NotifyRequestUpdatedAsync(RequestDto request)
    {
        var group = $"user-{request.SellerId}";
        await _hubContext.Clients.Group(group).SendAsync("RequestUpdated", request);
    }

    public async Task NotifyRequestCancelledAsync(Guid requestId, Guid sellerId)
    {
        var group = $"user-{sellerId}";
        await _hubContext.Clients.Group(group).SendAsync("RequestCancelled", requestId);
    }
}
